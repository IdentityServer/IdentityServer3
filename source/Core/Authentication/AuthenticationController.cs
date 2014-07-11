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
                var signIn = SaveSignInMessage(message);
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
                VerifySignInMessage();
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

            return SignInAndRedirect(
                 authResult,
                 Constants.AuthenticationMethods.Password,
                 Constants.BuiltInIdentityProvider);
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

            Logger.InfoFormat("external user provider: {0}, provider ID: {1}", externalIdentity.Provider, externalIdentity.ProviderId);
            
            var authResult = await _userService.AuthenticateExternalAsync(currentSubject, externalIdentity);
            if (authResult == null)
            {
                Logger.Warn("user service failed to authenticate external identity");
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                Logger.WarnFormat("user service returned error message: {0}", authResult.ErrorMessage);
                return RenderLoginPage(authResult.ErrorMessage);
            }

            return SignInAndRedirect(
                authResult,
                Constants.AuthenticationMethods.External,
                authResult.Provider);
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

            var subject = user.GetSubjectId();
            var name = user.GetName();
            var result = new AuthenticateResult(subject, name);
            
            var method = user.GetAuthenticationMethod();
            var idp = user.GetIdentityProvider();
            var authTime = user.GetAuthenticationTimeEpoch();

            return SignInAndRedirect(result, method, idp, authTime);
        }

        [Route(Constants.RoutePaths.Logout, Name = Constants.RouteNames.Logout)]
        [HttpGet, HttpPost]
        public async Task<IHttpActionResult> Logout()
        {
            var sub = await GetSubjectFromPrimaryAuthenticationType();
            Logger.InfoFormat("Logout requested for subject: {0}", sub);

            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            ClearSignInMessage();

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

            var ctx = Request.GetOwinContext();
            
            var signInMessage = LoadSignInMessage();
            Uri redirect = null;
            if (authResult.IsPartialSignIn)
            {
                redirect = new Uri(ctx.Request.Uri, authResult.PartialSignInRedirectPath.Value);
            }
            else
            {
                redirect = new Uri(signInMessage.ReturnUrl);

                // TODO -- manage this state better if we're doing redirect to custom page
                // would rather the redirect URL from request message put into cookie
                // and named with a nonce, then the resume url + nonce set as claim
                // in principal above so page being redirected to can know what url to return to
                ClearSignInMessage();
            }

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
                var resumeLoginUrl = Url.Route(Constants.RouteNames.ResumeLoginFromRedirect, null);
                var resumeLoginClaim = new Claim(Constants.ClaimTypes.PartialLoginReturnUrl, resumeLoginUrl);
                id.AddClaim(resumeLoginClaim);

                // allow redircting code to add claims for target page
                id.AddClaims(authResult.RedirectClaims);
            }

            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.PartialSignInAuthenticationType);

            Logger.InfoFormat("logging user in as subject: {0}, name: {1}{2}", authResult.Subject, authResult.Name, authResult.IsPartialSignIn?" (partial login)":"");
            ctx.Authentication.SignIn(id);

            Logger.InfoFormat("redirecting to: {0}", redirect.AbsoluteUri);
            
            return Redirect(redirect.AbsoluteUri);
        }

        private IHttpActionResult RenderLoginPage(string errorMessage = null, string username = null)
        {
            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                select new { name = p.Caption, url = Url.Route(Constants.RouteNames.LoginExternal, new { provider = p.AuthenticationType }) };

            var links = _authenticationOptions.LoginPageLinks;

            if (errorMessage != null)
            {
                Logger.InfoFormat("rendering login page with error message: {0}", errorMessage);
            }
            else
            {
                Logger.Info("rendering login page");
            }

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

            Logger.Info("rendering logout page");

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

        public const string SignInMessageCookieName = "idsrv.signin.message";
        private void ClearSignInMessage()
        {
            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                SignInMessageCookieName,
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
            var signInMessage = SignInMessage.Unprotect(
                message,
                _internalConfiguration.DataProtector);

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                SignInMessageCookieName,
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
            var message = ctx.Request.Cookies[SignInMessageCookieName];

            if (message.IsMissing())
            {
                Logger.Error("signin message cookie is empty");
                throw new Exception("SignInMessage cookie is empty.");
            }

            var signInMessage = SignInMessage.Unprotect(
                message,
                _internalConfiguration.DataProtector);

            return signInMessage;
        }

        private void VerifySignInMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[SignInMessageCookieName];

            if (message.IsMissing())
            {
                Logger.Error("signin message cookie is empty");
                throw new Exception("SignInMessage cookie is empty.");
            } 
            
            try
            {
                SignInMessage.Unprotect(
                    message,
                    _internalConfiguration.DataProtector);
            }
            catch 
            {
                Logger.Error("signin message failed to validate");
                throw;
            }
        }
    }
}