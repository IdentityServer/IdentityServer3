/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [ErrorPageFilter]
    public class AuthenticationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IUserService _userService;
        private readonly CoreSettings _settings;
        private readonly AuthenticationOptions _authenticationOptions;
        private readonly IExternalClaimsFilter _externalClaimsFilter;
        private readonly InternalConfiguration _internalConfiguration;

        public AuthenticationController(IUserService userService, CoreSettings settings, IExternalClaimsFilter externalClaimsFilter, AuthenticationOptions authenticationOptions, InternalConfiguration internalConfiguration)
        {
            _userService = userService;
            _settings = settings;
            _externalClaimsFilter = externalClaimsFilter;
            _authenticationOptions = authenticationOptions;
            _internalConfiguration = internalConfiguration;
        }

        [Route(Constants.RoutePaths.Login, Name = Constants.RouteNames.Login)]
        [HttpGet]
        public IHttpActionResult Login([FromUri] string message = null)
        {
            Logger.Info("Login page requested");

            if (message != null)
            {
                var signIn = SaveLoginRequestMessage(message);
                Logger.DebugFormat("SignInMessage passed to login: {0}", JsonConvert.SerializeObject(signIn, Formatting.Indented));

                if (signIn.IdP.IsPresent())
                {
                    Logger.InfoFormat("Identity provider requested, redirecting to: {0}", signIn.IdP);
                    return Redirect(Url.Link(Constants.RouteNames.LoginExternal, new { provider=signIn.IdP }));
                }
            }
            else
            {
                Logger.Debug("No SignInMessage passed to login; verifying message in cookie");
                VerifyLoginRequestMessage();
            }

            return RenderLoginPage();
        }

        [Route(Constants.RoutePaths.Login)]
        [HttpPost]
        public async Task<IHttpActionResult> LoginLocal(LoginCredentials model)
        {
            Logger.Info("Login page submitted");

            if (model == null)
            {
                Logger.Error("no data submitted");
                return RenderLoginPage(Messages.InvalidUsernameOrPassword);
            }

            if (!ModelState.IsValid)
            {
                Logger.Warn("validation error: username or password missing");
                return RenderLoginPage(ModelState.GetError(), model.Username);
            }

            var authResult = await _userService.AuthenticateLocalAsync(model.Username, model.Password);
            if (authResult == null)
            {
                Logger.WarnFormat("user service indicated incorrect username or password for username: {0}", model.Username);
                return RenderLoginPage(Messages.InvalidUsernameOrPassword, model.Username);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned an error message: {0}", authResult.ErrorMessage);
                return RenderLoginPage(authResult.ErrorMessage, model.Username);
            }

            Logger.InfoFormat("logging user in as subject: {0}, name: {1}", authResult.Subject, authResult.Name);

            return SignInAndRedirect(
                 authResult,
                 Constants.AuthenticationMethods.Password,
                 Constants.BuiltInIdentityProvider);
        }

        [Route(Constants.RoutePaths.LoginExternal, Name = Constants.RouteNames.LoginExternal)]
        [HttpGet]
        public IHttpActionResult LoginExternal(string provider)
        {
            Logger.InfoFormat("external login requested for provider: {0}", provider);

            VerifyLoginRequestMessage();

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
            Logger.Info("callback invoked from external identity provider ");

            var user = await GetIdentityFromExternalProvider();
            if (user == null)
            {
                Logger.Error("no identity from external identity provider");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            var externalIdentity = MapToExternalIdentity(user.Claims);
            if (externalIdentity == null)
            {
                Logger.Error("no subject or unique identifier claims from external identity provider");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            string currentSubject = await GetSubjectFromPrimaryAuthenticationType();
            if (!String.IsNullOrWhiteSpace(currentSubject))
            {
                Logger.DebugFormat("user is already logged in as: {0}", currentSubject);
            }
            
            var authResult = await _userService.AuthenticateExternalAsync(currentSubject, externalIdentity);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                Logger.Warn("user service failed returned error message");
                return RenderLoginPage(authResult.ErrorMessage);
            }

            Logger.InfoFormat("logging user in as subject: {0}, name: {1}", authResult.Subject, authResult.Name);

            return SignInAndRedirect(
                authResult,
                Constants.AuthenticationMethods.External,
                authResult.Provider);
        }

        [Route(Constants.RoutePaths.ResumeLoginFromRedirect, Name = Constants.RouteNames.ResumeLoginFromRedirect)]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect()
        {
            Logger.Info("Callback to resume login from partial login requested");

            var user = await GetIdentityFromPartialSignIn();
            if (user == null)
            {
                Logger.Error("no identity from partial login");
                return RedirectToRoute(Constants.RouteNames.Login, null);
            }

            var subject = user.GetSubjectId();
            var name = user.GetName();
            var result = new AuthenticateResult(subject, name);
            
            var method = user.GetAuthenticationMethod();
            var idp = user.GetIdentityProvider();
            var authTime = user.GetAuthenticationTimeEpoch();

            Logger.InfoFormat("logging user in as subject: {0}, name: {1}", subject, name);

            return SignInAndRedirect(result, method, idp, authTime);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpGet, HttpPost]
        public IHttpActionResult Logout()
        {
            var sub = GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout requested for subject: {0}", sub);

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            ClearLoginRequestMessage();

            return RenderLogoutPage();
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

        private IHttpActionResult SignInAndRedirect(
            AuthenticateResult authResult,
            string authenticationMethod,
            string identityProvider,
            long authTime = 0)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(identityProvider)) throw new ArgumentNullException("identityProvider");

            var signInMessage = LoadLoginRequestMessage();

            var issuer = authResult.IsPartialSignIn ?
                Constants.PartialSignInAuthenticationType :
                Constants.PrimaryAuthenticationType;

            var principal = IdentityServerPrincipal.Create(
                authResult.Subject,
                authResult.Name,
                authenticationMethod,
                identityProvider,
                issuer,
                authTime);

            var id = principal.Identities.First();

            if (authResult.IsPartialSignIn)
            {
                // TODO: put original return URL into cookie with a GUID ID
                // and put the ID as route param for the resume URL. then
                // we can always call ClearLoginRequestMessage()
                id.AddClaim(new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, Url.Route(Constants.RouteNames.ResumeLoginFromRedirect, null)));

                // allow redircting code to add claims for target page
                id.AddClaims(authResult.RedirectClaims);
            }

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);
            ctx.Authentication.SignIn(id);

            if (authResult.IsPartialSignIn)
            {
                Logger.Info("partial signin requested -- name: " + authResult.Name + ", subject: " + authResult.Subject);
                Logger.Info("redirecting to " + authResult.PartialSignInRedirectPath.Value);

                var uri = new Uri(ctx.Request.Uri, authResult.PartialSignInRedirectPath.Value);
                return Redirect(uri);
            }

            Logger.Info("normal signin requested -- name: " + authResult.Name + ", subject: " + authResult.Subject);
            Logger.Info("redirecting back to authorization endpoint");

            // TODO -- manage this state better if we're doing redirect to custom page
            // would rather the redirect URL from request message put into cookie
            // and named with a nonce, then the resume url + nonce set as claim
            // in principal above so page being redirected to can know what url to return to
            ClearLoginRequestMessage();

            return Redirect(signInMessage.ReturnUrl);
        }

        private IHttpActionResult RenderLoginPage(string errorMessage = null, string username = null)
        {
            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                select new { name = p.Caption, url = Url.Route(Constants.RouteNames.LoginExternal, new { provider = p.AuthenticationType }) };

            var links = _authenticationOptions.LoginPageLinks;

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Server = _settings.SiteName,
                    Page = "login",
                    ErrorMessage = errorMessage,
                    PageModel = new
                    {
                        url = Url.Route(Constants.RouteNames.Login, null),
                        username = username,
                        providers = providers.ToArray(),
                        links = links
                    }
                });
        }

        private IHttpActionResult RenderLogoutPage()
        {
            var baseUrl = Request.GetBaseUrl(_settings.PublicHostName);
            var urls = new List<string>();
            //foreach(var url in _internalConfiguration.PluginConfiguration.SignOutCallbackUrls)
            //{
            //    var tmp = url;
            //    if (tmp.StartsWith("/")) tmp = tmp.Substring(1);
            //    urls.Add(baseUrl + tmp);
            //}

            return new EmbeddedHtmlResult(Request,
                   new LayoutModel
                   {
                       Server = _settings.SiteName,
                       Page = "logout",
                       PageModel = new
                       {
                           signOutUrls = urls.ToArray()
                       }
                   });
        }

        public const string LoginRequestMessageCookieName = "idsrv.login.message";
        private void ClearLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                LoginRequestMessageCookieName,
                ".",
                new Microsoft.Owin.CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(-1),
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });
        }

        private SignInMessage SaveLoginRequestMessage(string message)
        {
            var signInMessage = SignInMessage.Unprotect(
                message,
                _internalConfiguration.DataProtector);

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                LoginRequestMessageCookieName,
                message,
                new Microsoft.Owin.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
                });

            return signInMessage;
        }

        private SignInMessage LoadLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            if (message.IsMissing())
            {
                throw new Exception("LoginRequestMessage cookie is empty.");
            }

            var signInMessage = SignInMessage.Unprotect(
                message,
                _internalConfiguration.DataProtector);

            return signInMessage;
        }

        private void VerifyLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            var signInMessage = SignInMessage.Unprotect(
                message,
                _internalConfiguration.DataProtector);
        }
    }
}