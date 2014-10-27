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
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    [ErrorPageFilter]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    public class AuthenticationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IViewService _viewService;
        private readonly IUserService _userService;
        private readonly AuthenticationOptions _authenticationOptions;
        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clientStore;

        public AuthenticationController(IViewService viewService, IUserService userService, AuthenticationOptions authenticationOptions, IdentityServerOptions idSvrOptions, IClientStore clientStore)
        {
            _viewService = viewService;
            _userService = userService;
            _authenticationOptions = authenticationOptions;
            _options = idSvrOptions;
            _clientStore = clientStore;
        }

        [Route(Constants.RoutePaths.Login, Name = Constants.RouteNames.Login)]
        [HttpGet]
        public async Task<IHttpActionResult> Login(string signin)
        {
            Logger.Info("Login page requested");

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage();
            }

            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            var signInMessage = cookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage();
            }

            Logger.DebugFormat("signin message passed to login: {0}", JsonConvert.SerializeObject(signInMessage, Formatting.Indented));

            var authResult = await _userService.PreAuthenticateAsync(Request.GetOwinEnvironment(), signInMessage);
            if (authResult != null)
            {
                if (authResult.IsError)
                {
                    Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                    return RenderErrorPage(authResult.ErrorMessage);
                }

                Logger.Info("user service returned a login result");
                return SignInAndRedirect(signInMessage, signin, authResult);
            }

            if (signInMessage.IdP.IsPresent())
            {
                Logger.InfoFormat("identity provider requested, redirecting to: {0}", signInMessage.IdP);
                return Redirect(Url.Link(Constants.RouteNames.LoginExternal, new { provider = signInMessage.IdP, signin }));
            }

            return await RenderLoginPage(signInMessage, signin);
        }

        [Route(Constants.RoutePaths.Login)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> LoginLocal(string signin, LoginCredentials model)
        {
            Logger.Info("Login page submitted");

            if (this._options.AuthenticationOptions.EnableLocalLogin == false)
            {
                Logger.Warn("EnableLocalLogin disabled -- returning 405 MethodNotAllowed");
                return StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage();
            }

            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            var signInMessage = cookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage();
            }

            if (model == null)
            {
                Logger.Error("no data submitted");
                return await RenderLoginPage(signInMessage, signin, Messages.InvalidUsernameOrPassword);
            }

            // the browser will only send 'true' if ther user has checked the checkbox
            // it will pass nothing if the user does not check the checkbox
            // this check here is to establish if the user deliberatly did not check the checkbox
            // or if the checkbox was not presented as an option (and thus AllowRememberMe is not allowed)
            // true means they did check it, false means they did not, null means they were not presented with the choice
            if (_options.AuthenticationOptions.CookieOptions.AllowRememberMe)
            {
                if (model.RememberMe != true)
                {
                    model.RememberMe = false;
                }
            }
            else
            {
                model.RememberMe = null;
            }

            if (!ModelState.IsValid)
            {
                Logger.Warn("validation error: username or password missing");
                return await RenderLoginPage(signInMessage, signin, ModelState.GetError(), model.Username, model.RememberMe == true);
            }

            var authResult = await _userService.AuthenticateLocalAsync(model.Username, model.Password, signInMessage);
            if (authResult == null)
            {
                Logger.WarnFormat("user service indicated incorrect username or password for username: {0}", model.Username);
                return await RenderLoginPage(signInMessage, signin, Messages.InvalidUsernameOrPassword, model.Username, model.RememberMe == true);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                return await RenderLoginPage(signInMessage, signin, authResult.ErrorMessage, model.Username, model.RememberMe == true);
            }

            return SignInAndRedirect(signInMessage, signin, authResult, model.RememberMe);
        }

        [Route(Constants.RoutePaths.LoginExternal, Name = Constants.RouteNames.LoginExternal)]
        [HttpGet]
        public IHttpActionResult LoginExternal(string signin, string provider)
        {
            Logger.InfoFormat("External login requested for provider: {0}", provider);

            if (provider.IsMissing())
            {
                Logger.Error("No provider passed");
                return RenderErrorPage();
            }

            if (signin.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage();
            }

            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            var signInMessage = cookie.Read(signin);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage();
            }

            var authProp = new Microsoft.Owin.Security.AuthenticationProperties
            {
                RedirectUri = Url.Route(Constants.RouteNames.LoginExternalCallback, null)
            };
            // add the id to the dictionary so we can recall the cookie id on the callback
            authProp.Dictionary.Add("signin", signin);
            authProp.Dictionary.Add("katanaAuthenticationType", provider);
            Request.GetOwinContext().Authentication.Challenge(authProp, provider);
            return Unauthorized();
        }

        [Route(Constants.RoutePaths.LoginExternalCallback, Name = Constants.RouteNames.LoginExternalCallback)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback()
        {
            Logger.Info("Callback invoked from external identity provider ");

            var signInId = await GetSignInIdFromExternalProvider();

            if (signInId.IsMissing())
            {
                Logger.Error("No signin id passed");
                return RenderErrorPage();
            }

            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            var signInMessage = cookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage();
            }

            var user = await GetIdentityFromExternalProvider();
            if (user == null)
            {
                Logger.Error("no identity from external identity provider");
                return await RenderLoginPage(signInMessage, signInId, Messages.NoMatchingExternalAccount);
            }

            var externalIdentity = ExternalIdentity.FromClaims(user.Claims);
            if (externalIdentity == null)
            {
                Logger.Error("no subject or unique identifier claims from external identity provider");
                return await RenderLoginPage(signInMessage, signInId, Messages.NoMatchingExternalAccount);
            }

            Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);

            var authResult = await _userService.AuthenticateExternalAsync(externalIdentity);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                return await RenderLoginPage(signInMessage, signInId, Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned error message: {0}", authResult.ErrorMessage);
                return await RenderLoginPage(signInMessage, signInId, authResult.ErrorMessage);
            }

            return SignInAndRedirect(signInMessage, signInId, authResult);
        }

        [Route(Constants.RoutePaths.ResumeLoginFromRedirect, Name = Constants.RouteNames.ResumeLoginFromRedirect)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect(string resume)
        {
            Logger.Info("Callback requested to resume login from partial login");

            if (resume.IsMissing())
            {
                Logger.Error("no resumeId passed");
                return RenderErrorPage();
            }

            var user = await GetIdentityFromPartialSignIn();
            if (user == null)
            {
                Logger.Error("no identity from partial login");
                return RenderErrorPage();
            }

            var type = GetClaimTypeForResumeId(resume);
            var resumeClaim = user.FindFirst(type);
            if (resumeClaim == null)
            {
                Logger.Error("no claim matching resumeId");
                return RenderErrorPage();
            }

            var signInId = resumeClaim.Value;
            if (signInId.IsMissing())
            {
                Logger.Error("No signin id found in resume claim");
                return RenderErrorPage();
            }

            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            var signInMessage = cookie.Read(signInId);
            if (signInMessage == null)
            {
                Logger.Error("No cookie matching signin id found");
                return RenderErrorPage();
            }

            AuthenticateResult result = null;
            var externalProviderClaim = user.FindFirst(Constants.ClaimTypes.ExternalProviderUserId);
            if (externalProviderClaim == null)
            {
                // the user/subject was known, so pass thru (without the redirect claims)
                user.RemoveClaim(user.FindFirst(Constants.ClaimTypes.PartialLoginReturnUrl));
                user.RemoveClaim(user.FindFirst(GetClaimTypeForResumeId(resume)));
                result = new AuthenticateResult(new ClaimsPrincipal(user));
            }
            else
            {
                // the user was not known, we need to re-execute AuthenticateExternalAsync
                // to obtain a subject to proceed
                var provider = externalProviderClaim.Issuer;
                var providerId = externalProviderClaim.Value;
                var externalId = new ExternalIdentity
                {
                    Provider = provider,
                    ProviderId = providerId,
                    Claims = user.Claims
                };

                result = await _userService.AuthenticateExternalAsync(externalId);

                if (result == null)
                {
                    Logger.Warn("user service failed to authenticate external identity");
                    return await RenderLoginPage(signInMessage, signInId, Messages.NoMatchingExternalAccount);
                }

                if (result.IsError)
                {
                    Logger.WarnFormat("user service returned error message: {0}", result.ErrorMessage);
                    return await RenderLoginPage(signInMessage, signInId, result.ErrorMessage);
                }
            }

            return SignInAndRedirect(signInMessage, signInId, result);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.LogoutPrompt)]
        [HttpGet]
        public async Task<IHttpActionResult> LogoutPrompt(string id = null)
        {
            var sub = await GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout prompt for subject: {0}", sub);

            if (!this._options.AuthenticationOptions.DisableSignOutPrompt)
            {
                return await RenderLogoutPromptPage(id);
            }

            Logger.InfoFormat("DisableSignOutPrompt set to true, performing logout");
            return await Logout(id);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IHttpActionResult> Logout(string id = null)
        {
            var sub = await GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout requested for subject: {0}", sub);

            ClearAuthenticationCookies();
            ClearSignInCookies();
            await SignOutOfExternalIdP();

            return await RenderLoggedOutPage(id);
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
            var id = await GetIdentityFrom(Constants.ExternalAuthenticationType);
            if (id != null)
            {
                // this is mapping from the external IdP's issuer to the name of the 
                // katana middleware that's registered in startup
                var result = await GetAuthenticationFrom(Constants.ExternalAuthenticationType);
                if (!result.Properties.Dictionary.Keys.Contains("katanaAuthenticationType"))
                {
                    Logger.Error("Missing katanaAuthenticationType in external callback");
                    throw new InvalidOperationException("Missing katanaAuthenticationType");
                }

                var provider = result.Properties.Dictionary["katanaAuthenticationType"];
                var newClaims = id.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, provider));
                id = new ClaimsIdentity(newClaims, id.AuthenticationType);
            }
            return id;
        }

        private async Task<string> GetSignInIdFromExternalProvider()
        {
            var result = await GetAuthenticationFrom(Constants.ExternalAuthenticationType);
            if (result != null)
            {
                string val = null;
                if (result.Properties.Dictionary.TryGetValue("signin", out val))
                {
                    return val;
                }
            }
            return null;
        }

        private async Task<ClaimsIdentity> GetIdentityFromPartialSignIn()
        {
            return await GetIdentityFrom(Constants.PartialSignInAuthenticationType);
        }

        private async Task<ClaimsIdentity> GetIdentityFrom(string type)
        {
            var result = await GetAuthenticationFrom(type);
            if (result != null &&
                result.Identity != null &&
                result.Identity.IsAuthenticated)
            {
                return result.Identity;
            }
            return null;
        }

        private async Task<Microsoft.Owin.Security.AuthenticateResult> GetAuthenticationFrom(string type)
        {
            var ctx = Request.GetOwinContext();
            var result = await ctx.Authentication.AuthenticateAsync(type);
            return result;
        }

        private IHttpActionResult SignInAndRedirect(SignInMessage signInMessage, string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            IssueAuthenticationCookie(signInMessage, signInMessageId, authResult, rememberMe);

            var redirectUrl = GetRedirectUrl(signInMessage, authResult);
            Logger.InfoFormat("redirecting to: {0}", redirectUrl);
            return Redirect(redirectUrl);
        }

        private void IssueAuthenticationCookie(SignInMessage signInMessage, string signInMessageId, AuthenticateResult authResult, bool? rememberMe = null)
        {
            if (signInMessage == null) throw new ArgumentNullException("signInId");
            if (authResult == null) throw new ArgumentNullException("authResult");

            Logger.InfoFormat("issuing cookie{0}", authResult.IsPartialSignIn ? " (partial login)" : "");

            var props = new Microsoft.Owin.Security.AuthenticationProperties();

            var id = authResult.User.Identities.First();
            if (authResult.IsPartialSignIn)
            {
                // add claim so partial redirect can return here to continue login
                // we need a random ID to resume, and this will be the query string
                // to match a claim added. the claim added will be the original 
                // signIn ID. 
                var resumeId = Guid.NewGuid().ToString("N");

                var resumeLoginUrl = Url.Link(Constants.RouteNames.ResumeLoginFromRedirect, new { resume = resumeId });
                var resumeLoginClaim = new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, resumeLoginUrl);
                id.AddClaim(resumeLoginClaim);
                id.AddClaim(new Claim(GetClaimTypeForResumeId(resumeId), signInMessageId));
            }
            else
            {
                ClearSignInCookie(signInMessageId);
            }

            if (!authResult.IsPartialSignIn)
            {
                // don't issue persistnt cookie if it's a partial signin
                if (rememberMe == true ||
                    (rememberMe != false && this._options.AuthenticationOptions.CookieOptions.IsPersistent))
                {
                    // only issue persistent cookie if user consents (rememberMe == true) or
                    // if server is configured to issue persistent cookies and user has not explicitly
                    // denied the rememberMe (false)
                    // if rememberMe is null, then user was not prompted for rememberMe
                    props.IsPersistent = true;
                    if (rememberMe == true)
                    {
                        var expires = DateTime.UtcNow.Add(_options.AuthenticationOptions.CookieOptions.RememberMeDuration);
                        props.ExpiresUtc = new DateTimeOffset(expires);
                    }
                }
            }

            ClearAuthenticationCookies();

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignIn(props, id);
        }

        private static string GetClaimTypeForResumeId(string resume)
        {
            return String.Format(Constants.ClaimTypes.PartialLoginResumeId, resume);
        }

        private Uri GetRedirectUrl(SignInMessage signInMessage, AuthenticateResult authResult)
        {
            if (signInMessage == null) throw new ArgumentNullException("signInMessage");
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
                return new Uri(signInMessage.ReturnUrl);
            }
        }

        private void ClearAuthenticationCookies()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);
        }

        private async Task SignOutOfExternalIdP()
        {
            // look for idp claim other than IdSvr
            // if present, then signout of it
            var user = await GetIdentityFromPrimaryAuthenticationType();
            if (user != null)
            {
                var idp = user.GetIdentityProvider();
                if (idp != Constants.BuiltInIdentityProvider)
                {
                    var ctx = Request.GetOwinContext();
                    ctx.Authentication.SignOut(idp);
                }
            }
        }

        private async Task<IHttpActionResult> RenderLoginPage(SignInMessage message, string signInMessageId, string errorMessage = null, string username = null, bool rememberMe = false)
        {
            if (message == null) throw new ArgumentNullException("message");

            var providers = await GetExternalProviders(message, signInMessageId);

            if (errorMessage != null)
            {
                Logger.InfoFormat("rendering login page with error message: {0}", errorMessage);
            }
            else
            {
                if (_authenticationOptions.EnableLocalLogin == false && providers.Count() == 1)
                {
                    // no local login and only one provider -- redirect to provider
                    Logger.Info("no local login and only one provider -- redirect to provider");
                    var url = Request.GetOwinEnvironment().GetIdentityServerHost();
                    url += providers.First().Href;
                    return Redirect(url);
                }
                else
                {
                    Logger.Info("rendering login page");
                }
            }

            var loginPageLinks = PrepareLoginPageLinks(signInMessageId, _authenticationOptions.LoginPageLinks);

            var loginModel = new LoginViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = Request.GetIdentityServerBaseUrl(),
                CurrentUser = await GetNameFromPrimaryAuthenticationType(),
                ExternalProviders = providers,
                AdditionalLinks = loginPageLinks,
                ErrorMessage = errorMessage,
                LoginUrl = _options.AuthenticationOptions.EnableLocalLogin ? Url.Route(Constants.RouteNames.Login, new { signin = signInMessageId }) : null,
                AllowRememberMe = _options.AuthenticationOptions.CookieOptions.AllowRememberMe,
                RememberMe = _options.AuthenticationOptions.CookieOptions.AllowRememberMe && rememberMe,
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, null),
                AntiForgery = AntiForgeryTokenValidator.GetAntiForgeryHiddenInput(Request.GetOwinEnvironment()),
                Username = username
            };

            return new LoginActionResult(_viewService, Request.GetOwinEnvironment(), loginModel, message);
        }

        private async Task<IEnumerable<LoginPageLink>> GetExternalProviders(SignInMessage message, string signInMessageId)
        {
            IEnumerable<string> filter = null;
            if (!String.IsNullOrWhiteSpace(message.ClientId))
            {
                var client = await _clientStore.FindClientByIdAsync(message.ClientId);
                if (client == null) throw new InvalidOperationException("Invalid client: " + message.ClientId);
                filter = client.IdentityProviderRestrictions ?? filter;
            }
            filter = filter ?? Enumerable.Empty<string>();

            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                where (!filter.Any() || filter.Contains(p.AuthenticationType))
                select new LoginPageLink
                {
                    Text = p.Caption,
                    Href = Url.Route(Constants.RouteNames.LoginExternal,
                    new { provider = p.AuthenticationType, signin = signInMessageId })
                };

            return providers.ToArray();
        }

        private IEnumerable<LoginPageLink> PrepareLoginPageLinks(string signin, IEnumerable<LoginPageLink> links)
        {
            if (links == null || !links.Any()) return null;

            var result = new List<LoginPageLink>();
            foreach (var link in links)
            {
                var url = link.Href;
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                    url = Request.GetIdentityServerBaseUrl() + url;
                }

                url = url.AddQueryString("signin=" + signin);

                result.Add(new LoginPageLink
                {
                    Text = link.Text,
                    Href = url
                });
            }
            return result;
        }

        private async Task<IHttpActionResult> RenderLogoutPromptPage(string id = null)
        {
            var clientName = await GetClientNameFromSignOutMessageId(id);

            var env = Request.GetOwinEnvironment();
            var logoutModel = new LogoutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                CurrentUser = await GetNameFromPrimaryAuthenticationType(),
                LogoutUrl = Url.Route(Constants.RouteNames.Logout, new { id = id }),
                AntiForgery = AntiForgeryTokenValidator.GetAntiForgeryHiddenInput(Request.GetOwinEnvironment()),
                ClientName = clientName
            };
            return new LogoutActionResult(_viewService, env, logoutModel);
        }

        private async Task<IHttpActionResult> RenderLoggedOutPage(string id)
        {
            Logger.Info("rendering logged out page");

            var env = Request.GetOwinEnvironment();
            var baseUrl = env.GetIdentityServerBaseUrl();
            var urls = new List<string>();

            foreach (var url in _options.ProtocolLogoutUrls)
            {
                var tmp = url;
                if (tmp.StartsWith("/")) tmp = tmp.Substring(1);
                urls.Add(baseUrl + tmp);
            }

            var message = GetSignOutMessage(id);
            var redirectUrl = message != null ? message.ReturnUrl : null;
            var clientName = await GetClientNameFromSignOutMessage(message);
            ClearSignOutMessage();

            var loggedOutModel = new LoggedOutViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = baseUrl,
                IFrameUrls = urls,
                ClientName = clientName,
                RedirectUrl = redirectUrl
            };
            return new LoggedOutActionResult(_viewService, env, loggedOutModel);
        }

        private void ClearSignOutMessage()
        {
            var cookie = new MessageCookie<SignOutMessage>(Request.GetOwinContext(), this._options);
            cookie.ClearAll();
        }

        private SignOutMessage GetSignOutMessage(string id)
        {
            if (!String.IsNullOrWhiteSpace(id))
            {
                var cookie = new MessageCookie<SignOutMessage>(Request.GetOwinContext(), this._options);
                return cookie.Read(id);
            }
            return null;
        }

        private async Task<string> GetClientNameFromSignOutMessageId(string id)
        {
            var signOutMessage = GetSignOutMessage(id);
            return await GetClientNameFromSignOutMessage(signOutMessage);
        }

        private async Task<string> GetClientNameFromSignOutMessage(SignOutMessage signOutMessage)
        {
            if (signOutMessage != null)
            {
                var client = await _clientStore.FindClientByIdAsync(signOutMessage.ClientId);
                if (client != null)
                {
                    return client.ClientName;
                }
            }
            return null;
        }

        private IHttpActionResult RenderErrorPage(string message = null)
        {
            message = message ?? Resources.Messages.UnexpectedError;
            var errorModel = new ErrorViewModel
            {
                SiteName = this._options.SiteName,
                SiteUrl = Request.GetOwinContext().Environment.GetIdentityServerBaseUrl(),
                ErrorMessage = message
            };
            var errorResult = new ErrorActionResult(_viewService, Request.GetOwinContext().Environment, errorModel);
            return errorResult;
        }

        private void ClearSignInCookies()
        {
            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            cookie.ClearAll();
        }

        private void ClearSignInCookie(string signin)
        {
            var cookie = new MessageCookie<SignInMessage>(Request.GetOwinContext(), this._options);
            cookie.Clear(signin);
        }

    }
}