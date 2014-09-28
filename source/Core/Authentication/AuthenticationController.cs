/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [ErrorPageFilter]
    [SecurityHeaders]
    [NoCache]
    public class AuthenticationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IViewService _viewService;
        private readonly IUserService _userService;
        private readonly AuthenticationOptions _authenticationOptions;
        private readonly IExternalClaimsFilter _externalClaimsFilter;
        private readonly IdentityServerOptions _options;

        public AuthenticationController(IViewService viewService, IUserService userService, IExternalClaimsFilter externalClaimsFilter, AuthenticationOptions authenticationOptions, IdentityServerOptions idSvrOptions)
        {
            _viewService = viewService;
            _userService = userService;
            _externalClaimsFilter = externalClaimsFilter;
            _authenticationOptions = authenticationOptions;
            _options = idSvrOptions;
        }

        [Route(Constants.RoutePaths.Login, Name = Constants.RouteNames.Login)]
        [HttpGet]
        public async Task<IHttpActionResult> Login([FromUri] string message = null)
        {
            Logger.Info("Login page requested");

            SignInMessage signIn = null;
            if (message != null)
            {
                signIn = SaveSignInMessage(message);
                Logger.DebugFormat("signin message passed to login: {0}", JsonConvert.SerializeObject(signIn, Formatting.Indented));

                if (signIn.IdP.IsPresent())
                {
                    Logger.InfoFormat("identity provider requested, redirecting to: {0}", signIn.IdP);
                    return Redirect(Url.Link(Constants.RouteNames.LoginExternal, new { provider=signIn.IdP }));
                }
            }
            else
            {
                Logger.Debug("no signin message passed to login -- verifying message in cookie");
                signIn = LoadSignInMessage();
            }

            return await RenderLoginPage(message:signIn);
        }

        [Route(Constants.RoutePaths.Login)]
        [HttpPost]
        public async Task<IHttpActionResult> LoginLocal(LoginCredentials model)
        {
            Logger.Info("Login page submitted");

            if (model == null)
            {
                Logger.Error("no data submitted");
                return await RenderLoginPage(Messages.InvalidUsernameOrPassword);
            }

            if (!ModelState.IsValid)
            {
                Logger.Warn("validation error: username or password missing");
                return await RenderLoginPage(ModelState.GetError(), model.Username);
            }

            var authResult = await _userService.AuthenticateLocalAsync(model.Username, model.Password, LoadSignInMessage());
            if (authResult == null)
            {
                Logger.WarnFormat("user service indicated incorrect username or password for username: {0}", model.Username);
                return await RenderLoginPage(Messages.InvalidUsernameOrPassword, model.Username);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                return await RenderLoginPage(authResult.ErrorMessage, model.Username);
            }

            return SignInAndRedirect(authResult);
        }

        [Route(Constants.RoutePaths.LoginExternal, Name = Constants.RouteNames.LoginExternal)]
        [HttpGet]
        public IHttpActionResult LoginExternal(string provider)
        {
            Logger.InfoFormat("External login requested for provider: {0}", provider);

            VerifySignInMessage();

            var ctx = Request.GetOwinContext();
            var authProp = new Microsoft.Owin.Security.AuthenticationProperties
            {
                RedirectUri = Url.Route(Constants.RouteNames.LoginExternalCallback, null)
            };
            Request.GetOwinContext().Authentication.Challenge(authProp, provider);
            return Unauthorized();
        }

        [Route(Constants.RoutePaths.LoginExternalCallback, Name = Constants.RouteNames.LoginExternalCallback)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback()
        {
            Logger.Info("Callback invoked from external identity provider ");

            var user = await GetIdentityFromExternalProvider();
            if (user == null)
            {
                Logger.Error("no identity from external identity provider");
                return await RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            var externalIdentity = MapToExternalIdentity(user.Claims);
            if (externalIdentity == null)
            {
                Logger.Error("no subject or unique identifier claims from external identity provider");
                return await RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);
            
            var authResult = await _userService.AuthenticateExternalAsync(externalIdentity);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                return await RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned error message: {0}", authResult.ErrorMessage);
                return await RenderLoginPage(authResult.ErrorMessage);
            }

            return SignInAndRedirect(authResult);
        }

        [Route(Constants.RoutePaths.ResumeLoginFromRedirect, Name = Constants.RouteNames.ResumeLoginFromRedirect)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect()
        {
            Logger.Info("Callback requested to resume login from partial login");

            var user = await GetIdentityFromPartialSignIn();
            if (user == null)
            {
                Logger.Error("no identity from partial login");
                return RedirectToRoute(Constants.RouteNames.Login, null);
            }

            AuthenticateResult result = null;
            var externalProviderClaim = user.FindFirst(Constants.ClaimTypes.ExternalProviderUserId);
            if (externalProviderClaim == null)
            {
                // the user/subject was known, so pass thru
                result = new AuthenticateResult(new ClaimsPrincipal(user));
            }
            else
            {
                // the user was not known, we need to re-execute AuthenticateExternalAsync
                // to obtain a subject to proceed
                var provider = externalProviderClaim.Issuer;
                var providerId = externalProviderClaim.Value;
                var externalId = new ExternalIdentity()
                {
                    Provider = new IdentityProvider{ Name = provider },
                    ProviderId = providerId,
                    Claims = user.Claims
                };

                result = await _userService.AuthenticateExternalAsync(externalId);

                if (result == null)
                {
                    Logger.Warn("user service failed to authenticate external identity");
                    return await RenderLoginPage(Messages.NoMatchingExternalAccount);
                }

                if (result.IsError)
                {
                    Logger.WarnFormat("user service returned error message: {0}", result.ErrorMessage);
                    return await RenderLoginPage(result.ErrorMessage);
                }
            }

            return SignInAndRedirect(result);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public async Task<IHttpActionResult> LogoutPrompt()
        {
            var sub = await GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout prompt for subject: {0}", sub);

            return await RenderLogoutPromptPage();
        }
        
        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpPost]
        public async Task<IHttpActionResult> Logout()
        {
            var sub = await GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout requested for subject: {0}", sub);

            ClearAuthenticationCookies();
            ClearSignInMessage();

            return RenderLoggedOutPage();
        }

        private async Task<string> GetSubjectFromPrimaryAuthenticationType()
        {
            var user = await GetIdentityFromPrimaryAuthenticationType();
            if (user != null)
            {
                return user.Claims.GetValue(Constants.ClaimTypes.Subject);
            }
            return null;
        }
        
        private async Task<string> GetNameFromPrimaryAuthenticationType()
        {
            var user = await GetIdentityFromPrimaryAuthenticationType();
            if (user != null)
            {
                return user.Claims.GetValue(Constants.ClaimTypes.Name);
            }
            return null;
        }

        private async Task<ClaimsIdentity> GetIdentityFromPrimaryAuthenticationType()
        {
            return await GetIdentityFrom(Constants.PrimaryAuthenticationType);
        }
        
        private async Task<ClaimsIdentity> GetIdentityFromExternalProvider()
        {
            return await GetIdentityFrom(Constants.ExternalAuthenticationType);
        }

        private async Task<ClaimsIdentity> GetIdentityFromPartialSignIn()
        {
            return await GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        private async Task<ClaimsIdentity> GetIdentityFrom(string type)
        {
            var ctx = Request.GetOwinContext();
            var result = await ctx.Authentication.AuthenticateAsync(type);
            if (result != null &&
                result.Identity != null &&
                result.Identity.IsAuthenticated)
            {
                return result.Identity;
            }
            return null;
        }

        private ExternalIdentity MapToExternalIdentity(IEnumerable<Claim> claims)
        {
            var externalId = ExternalIdentity.FromClaims(claims);
            if (externalId != null && _externalClaimsFilter != null)
            {
                externalId.Claims = _externalClaimsFilter.Filter(externalId.Provider, externalId.Claims);
            }
            return externalId;
        }

        private IHttpActionResult SignInAndRedirect(AuthenticateResult authResult)
        {
            IssueAuthenticationCookie(authResult);

            var redirectUrl = GetRedirectUrl(authResult);
            Logger.InfoFormat("redirecting to: {0}", redirectUrl);
            return Redirect(redirectUrl);
        }

        private Uri GetRedirectUrl(AuthenticateResult authResult)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");

            if (authResult.IsPartialSignIn)
            {
                var url = authResult.PartialSignInRedirectPath;
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                    url = Request.GetIdentityServerBaseUrl() + url;
                }
                return new Uri(Request.RequestUri, url);
            }
            else
            {
                var signInMessage = LoadSignInMessage(); 
                return new Uri(signInMessage.ReturnUrl);
            }
        }

        private void IssueAuthenticationCookie(AuthenticateResult authResult)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");
            
            Logger.InfoFormat("logging user in as subject: {0}, name: {1}{2}", authResult.User.GetSubjectId(), authResult.User.GetName(), authResult.IsPartialSignIn ? " (partial login)" : "");
            
            var props = new Microsoft.Owin.Security.AuthenticationProperties();

            var id = authResult.User.Identities.First();
            if (authResult.IsPartialSignIn)
            {
                // add claim so partial redirect can return here to continue login
                var resumeLoginUrl = Url.Link(Constants.RouteNames.ResumeLoginFromRedirect, null);
                var resumeLoginClaim = new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, resumeLoginUrl);
                id.AddClaim(resumeLoginClaim);
            }
            else
            {
                ClearSignInMessage();
            }

            if (!authResult.IsPartialSignIn && this._options.CookieOptions.IsPersistent)
            {
                props.IsPersistent = true;
            }

            ClearAuthenticationCookies();

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignIn(props, id);
        }

        private void ClearAuthenticationCookies()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);
        }

        private async Task<IHttpActionResult> RenderLoginPage(string errorMessage = null, string username = null, SignInMessage message = null)
        {
            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                select new LoginPageLink{ Text = p.Caption, Href = Url.Route(Constants.RouteNames.LoginExternal, new { provider = p.AuthenticationType }) };

            if (errorMessage != null)
            {
                Logger.InfoFormat("rendering login page with error message: {0}", errorMessage);
            }
            else
            {
                Logger.Info("rendering login page");
            }

            var loginPageLinks = PrepareLoginPageLinks(_authenticationOptions.LoginPageLinks);

            var loginModel = new LoginViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = ctx.Environment.GetIdentityServerBaseUrl(),
                CurrentUser = await GetNameFromPrimaryAuthenticationType(), 
                ExternalProviders = providers,
                AdditionalLinks = loginPageLinks,
                ErrorMessage = errorMessage,
                LoginUrl = Url.Route(Constants.RouteNames.Login, null),
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, null),
                Username = username
            };

            return new LoginActionResult(_viewService, ctx.Environment, loginModel, message ?? LoadSignInMessage());
        }

        private IEnumerable<LoginPageLink> PrepareLoginPageLinks(IEnumerable<LoginPageLink> links)
        {
            if (links == null || !links.Any()) return null;

            var result = new List<LoginPageLink>();
            foreach(var link in links)
            {
                var url = link.Href;
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                    url = Request.GetIdentityServerBaseUrl() + url;
                }
                result.Add(new LoginPageLink
                {
                    Text = link.Text, Href = url
                });
            }
            return result;
        }

        private async Task<IHttpActionResult> RenderLogoutPromptPage()
        {
            var env = Request.GetOwinEnvironment();
            var logoutModel = new LogoutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = await GetNameFromPrimaryAuthenticationType(),
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, null),
            };
            return new LogoutActionResult(_viewService, env, logoutModel);
        }

        private IHttpActionResult RenderLoggedOutPage()
        {
            var env = Request.GetOwinEnvironment();
            var baseUrl = env.GetIdentityServerBaseUrl();
            var urls = new List<string>();

            foreach (var url in _options.ProtocolLogoutUrls)
            {
                var tmp = url;
                if (tmp.StartsWith("/")) tmp = tmp.Substring(1);
                urls.Add(baseUrl + tmp);
            }

            Logger.Info("rendering logged out page");

            var loggedOutModel = new LoggedOutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = baseUrl,
                IFrameUrls = urls,
            };
            return new LoggedOutActionResult(_viewService, env, loggedOutModel);
        }

        public const string SignInMessageCookieName = "idsrv.signin.message";
        private void ClearSignInMessage()
        {
            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                _options.CookieOptions.Prefix + SignInMessageCookieName,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });
        }

        private SignInMessage SaveSignInMessage(string message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var signInMessage = SignInMessage.Unprotect(
                message,
                _options.DataProtector);

            CheckExpired(signInMessage);

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                _options.CookieOptions.Prefix + SignInMessageCookieName,
                message,
                new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });

            return signInMessage;
        }

        private SignInMessage LoadSignInMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[_options.CookieOptions.Prefix + SignInMessageCookieName];

            if (message.IsMissing())
            {
                Logger.Error("signin message cookie is empty");
                throw new Exception("SignInMessage cookie is empty.");
            }

            try
            {
                return SignInMessage.Unprotect(
                    message,
                    _options.DataProtector);
            }
            catch
            {
                Logger.Error("signin message failed to validate");
                throw;
            }
        }

        private void VerifySignInMessage()
        {
            LoadSignInMessage();
        }

        private void CheckExpired(SignInMessage signInMessage)
        {
            if (signInMessage.IsExpired)
            {
                Logger.Error("signin message is expired; redirecting back to authorization endpoint");

                var response = Request.CreateResponse(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(signInMessage.ReturnUrl);
                throw new HttpResponseException(response);
            }
        }
    }
}