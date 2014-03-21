using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class Credentials
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class ErrorPageFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var response = EmbeddedHtmlResult.GetResponseMessage(
                actionExecutedContext.Request, 
                new LayoutModel
                {
                    Page = "error"
                });

            actionExecutedContext.Response = response;
        }
    }

    [ErrorPageFilter]
    public class LoginController : ApiController
    {
        IUserService userService;
        private ICoreSettings _settings;

        public LoginController(IUserService userService, ICoreSettings settings)
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
        public IHttpActionResult LoginLocal(Credentials model)
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
            var authResult = await ctx.Authentication.AuthenticateAsync("idsrv.external");
            if (authResult == null)
            {
                return RedirectToRoute("login", null);
            }

            return SignInAndRedirect(new Thinktecture.IdentityServer.Core.Services.AuthenticateResult
            {
                Subject = authResult.Identity.Name, Username = authResult.Identity.Name
            });
        }

        [Route("logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(Constants.BuiltInAuthenticationType, "idsrv.external");
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
               Constants.AuthenticationMethods.Password,
               Constants.BuiltInIdentityProvider);

            var id = principal.Identities.First();
            id.AddClaim(new Claim(ClaimTypes.Name, authResult.Username));
            ctx.Authentication.SignIn(id);

            ctx.Authentication.SignOut("idsrv.external");
            ClearLoginRequestMessage();

            return Redirect(signInMessage.ReturnUrl);
        }

        //[Route("providers")]
        //public IHttpActionResult GetProviders()
        //{
        //    var ctx = Request.GetOwinContext();
        //    var providers =
        //        from p in ctx.Authentication.GetAuthenticationTypes(d => !String.IsNullOrWhiteSpace(d.Caption))
        //        select new { name = p.Caption, url = Url.Link("SigninExternal", new { provider = p.AuthenticationType }) };


        //    //foreach (var provider in providers)
        //    //{
        //    //    ExternalLoginViewModel login = new ExternalLoginViewModel
        //    //    {
        //    //        Name = description.Caption,
        //    //        Url = Url.Route("ExternalLogin", new
        //    //        {
        //    //            provider = description.AuthenticationType,
        //    //            response_type = "token",
        //    //            client_id = Startup.PublicClientId,
        //    //            redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //    //            state = state
        //    //        }),
        //    //        State = state
        //    //    };
        //    //    logins.Add(login);
        //    //} 

        //    return Ok(providers);
        //}


        private IHttpActionResult RenderLoginPage(string errorMessage = null, string username = null)
        {
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
                        providers = new[] { 
                            new {
                                name="Google", 
                                url=Url.Route("external", new{provider="Google"}) 
                            },
                            new {
                                name="Facebook", 
                                url=Url.Route("external", new{provider="Facebook"}) 
                            },
                        }
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
