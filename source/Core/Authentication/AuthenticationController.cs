/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [ErrorPageFilter]
    public class AuthenticationController : ApiController
    {
        IUserService userService;
        private ICoreSettings _settings;

        public AuthenticationController(IUserService userService, ICoreSettings settings)
        {
            this.userService = userService;
            _settings = settings;
        }

        [Route("login", Name="login")]
        [HttpGet]
        public IHttpActionResult Login([FromUri] string message = null)
        {
            if (message != null)
            {
                SaveLoginRequestMessage(message);
            }
            else
            {
                VerifyLoginRequestMessage();
            }

            return RenderLoginPage();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IHttpActionResult> LoginLocal(LoginCredentials model)
        {
            if (model == null)
            {
                return RenderLoginPage(Messages.InvalidUsernameOrPassword);
            }

            if (!ModelState.IsValid)
            {
                return RenderLoginPage(ModelState.GetError(), model.Username);
            }

            var authResult = await userService.AuthenticateLocalAsync(model.Username, model.Password);
            if (authResult == null)
            {
                return RenderLoginPage(Messages.InvalidUsernameOrPassword, model.Username);
            }
            
            if (authResult.IsError)
            {
                return RenderLoginPage(authResult.ErrorMessage, model.Username);
            }

           return SignInAndRedirect(
                authResult, 
                Constants.AuthenticationMethods.Password, 
                Constants.BuiltInIdentityProvider);
        }

        [Route("external", Name="external")]
        [HttpGet]
        public IHttpActionResult LoginExternal(string provider)
        {
            VerifyLoginRequestMessage();

            var ctx = Request.GetOwinContext();
            var authProp = new AuthenticationProperties {
                RedirectUri = Url.Route("callback", null)
            };
            Request.GetOwinContext().Authentication.Challenge(authProp, provider);
            return Unauthorized();
        }

        [Route("callback", Name = "callback")]
        [HttpGet]
        public async Task<IHttpActionResult> LoginExternalCallback()
        {
            var ctx = Request.GetOwinContext();

            string currentSubject = null;
            var currentAuth = await ctx.Authentication.AuthenticateAsync(Constants.PrimaryAuthenticationType);
            if (currentAuth != null &&
                currentAuth.Identity != null &&
                currentAuth.Identity.IsAuthenticated)
            {
                currentSubject = currentAuth.Identity.Claims.GetValue(Constants.ClaimTypes.Subject);
            }

            var externalAuthResult = await ctx.Authentication.AuthenticateAsync(Constants.ExternalAuthenticationType);
            if (externalAuthResult == null ||
                externalAuthResult.Identity == null ||
                !externalAuthResult.Identity.Claims.Any())
            {
                return RedirectToRoute("login", null);
            }

            var claims = externalAuthResult.Identity.Claims;
            var authResult = await userService.AuthenticateExternalAsync(currentSubject, claims);
            if (authResult == null)
            {
                return RenderLoginPage(Messages.NoMatchingExternalAccount);
            }

            if (authResult.IsError)
            {
                return RenderLoginPage(authResult.ErrorMessage);
            }

            return SignInAndRedirect(
                authResult, 
                Constants.AuthenticationMethods.External, 
                authResult.Provider);
        }

        [Route("logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType,
                Constants.RedirectAuthenticationType);

            ClearLoginRequestMessage();

            return new EmbeddedHtmlResult(Request,
                   new LayoutModel
                   {
                       Title = _settings.GetSiteName(),
                       Page = "logout"
                   });
        }

        [Route("resume")]
        [HttpGet]
        public async Task<IHttpActionResult> ResumeLoginFromRedirect()
        {
            var ctx = Request.GetOwinContext();
            var redirectAuthResult = await ctx.Authentication.AuthenticateAsync(Constants.RedirectAuthenticationType);
            if (redirectAuthResult == null ||
                redirectAuthResult.Identity == null)
            {
                return RedirectToRoute("login", null);
            }

            var subject = redirectAuthResult.Identity.GetSubjectId();
            var name = redirectAuthResult.Identity.GetName();
            
            var result = new Thinktecture.IdentityServer.Core.Services.AuthenticateResult(subject, name);
            var method = redirectAuthResult.Identity.GetAuthenticationMethod();
            var idp = redirectAuthResult.Identity.GetIdentityProvider();
            var authTime = redirectAuthResult.Identity.GetAuthenticationTimeEpoch();

            return SignInAndRedirect(result, method, idp, authTime);
        }
        
        private IHttpActionResult SignInAndRedirect(
            Thinktecture.IdentityServer.Core.Services.AuthenticateResult authResult, 
            string authenticationMethod, 
            string identityProvider,
            long authTime = 0)
        {
            if (authResult == null) throw new ArgumentNullException("authResult");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(identityProvider)) throw new ArgumentNullException("identityProvider");

            var signInMessage = LoadLoginRequestMessage();
            var issuer = authResult.IsRedirect ? Constants.RedirectAuthenticationType : Constants.PrimaryAuthenticationType;

            var principal = IdentityServerPrincipal.Create(
               authResult.Subject,
               authResult.Name,
               authenticationMethod,
               identityProvider,
               issuer, 
               authTime);
            
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(
                Constants.PrimaryAuthenticationType,
                Constants.ExternalAuthenticationType, 
                Constants.RedirectAuthenticationType);

            var id = principal.Identities.First();

            if (authResult.IsRedirect)
            {
                // allow redircting code to add claims for target page
                id.AddClaims(authResult.RedirectClaims);
            }

            ctx.Authentication.SignIn(id);
            
            if (authResult.IsRedirect)
            {
                var uri = new Uri(ctx.Request.Uri, authResult.RedirectPath.Value);
                return Redirect(uri);
            }
            else
            {
                // TODO -- manage this state better if we're doing redirect to custom page
                // would rather the redirect URL from request message put into cookie
                // and named with a nonce, then the resume url + nonce set as claim
                // in principal above so page being redirected to can know what url to return to
                ClearLoginRequestMessage();

                return Redirect(signInMessage.ReturnUrl);
            }
        }

        private IHttpActionResult RenderLoginPage(string errorMessage = null, string username = null)
        {
            var ctx = Request.GetOwinContext();
            var providers =
                from p in ctx.Authentication.GetAuthenticationTypes(d => d.Caption.IsPresent())
                select new { name = p.Caption, url = Url.Route("external", new { provider = p.AuthenticationType }) };

            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel
                {
                    Title = _settings.GetSiteName(),
                    Page = "login",
                    ErrorMessage = errorMessage,
                    PageModel = new
                    {
                        url = Url.Route("login", null),
                        username = username,
                        providers = providers.ToArray()
                    }
                });
        }

        const string LoginRequestMessageCookieName = "idsrv.login.message";
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
#if DEBUG
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
#else
                    Secure = true
#endif
                });
        }

        private void SaveLoginRequestMessage(string message)
        {
            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);

            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                LoginRequestMessageCookieName, 
                message, 
                new Microsoft.Owin.CookieOptions {
                    HttpOnly = true,
#if DEBUG
                    Secure = Request.RequestUri.Scheme == Uri.UriSchemeHttps
#else
                    Secure = true
#endif
                });
        }

        private SignInMessage LoadLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);

            return signInMessage;
        }

        private void VerifyLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            var message = ctx.Request.Cookies[LoginRequestMessageCookieName];

            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);
        }
    }
}
