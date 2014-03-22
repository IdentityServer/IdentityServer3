using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Plumbing;
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
        public IHttpActionResult LoginLocal(LoginCredentials model)
        {
            VerifyLoginRequestMessage();
            
            if (model == null)
            {
                return RenderLoginPage("Invalid Username or Password");
            }

            if (!ModelState.IsValid)
            {
                var error =
                    from item in ModelState
                    where item.Value.Errors.Any()
                    from err in item.Value.Errors
                    select err.ErrorMessage;
                return RenderLoginPage(error.First(), model.Username);
            }

            var authResult = userService.Authenticate(model.Username, model.Password);
            if (authResult == null)
            {
                return RenderLoginPage("Invalid Username or Password", model.Username);
            }

            return SignInAndRedirect(authResult);
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
            VerifyLoginRequestMessage();

            var ctx = Request.GetOwinContext();
            var externalAuthResult = await ctx.Authentication.AuthenticateAsync(Constants.ExternalAuthenticationType);
            if (externalAuthResult == null ||
                externalAuthResult.Identity == null ||
                !externalAuthResult.Identity.Claims.Any())
            {
                return RedirectToRoute("login", null);
            }

            string currentSubject = null;
            var currentAuth = await ctx.Authentication.AuthenticateAsync(Constants.BuiltInAuthenticationType);
            if (currentAuth != null && 
                currentAuth.Identity != null && 
                currentAuth.Identity.IsAuthenticated)
            {
                currentSubject = currentAuth.Identity.Claims.GetValue(Constants.ClaimTypes.Subject);
            }

            var authResult = userService.Authenticate(currentSubject, externalAuthResult.Identity.Claims);
            if (authResult == null)
            {
                return RenderLoginPage("Invalid Account");
            }

            return SignInAndRedirect(authResult);
        }

        [Route("logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(Constants.BuiltInAuthenticationType, Constants.ExternalAuthenticationType);
            ClearLoginRequestMessage();

            return new EmbeddedHtmlResult(Request,
                   new LayoutModel
                   {
                       Title = _settings.GetSiteName(),
                       Page = "logout"
                   });
        }
        
        private IHttpActionResult SignInAndRedirect(Thinktecture.IdentityServer.Core.Services.AuthenticateResult authResult)
        {
            var signInMessage = LoadLoginRequestMessage();
            var ctx = Request.GetOwinContext();

            var principal = IdentityServerPrincipal.Create(
               authResult.Subject,
               authResult.Name,
               Constants.AuthenticationMethods.Password,
               Constants.BuiltInIdentityProvider);
            ctx.Authentication.SignIn(principal.Identities.First());

            ctx.Authentication.SignOut(Constants.ExternalAuthenticationType);
            ClearLoginRequestMessage();

            return Redirect(signInMessage.ReturnUrl);
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

        private void ClearLoginRequestMessage()
        {
            var ctx = Request.GetOwinContext();
            ctx.Response.Cookies.Append(
                "idsrv.login.message",
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
                "idsrv.login.message", 
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
            var message = ctx.Request.Cookies["idsrv.login.message"];

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
            var message = ctx.Request.Cookies["idsrv.login.message"];

            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);
        }
    }
}
