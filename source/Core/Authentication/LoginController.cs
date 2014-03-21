using Microsoft.Owin.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
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
        public IHttpActionResult Get([FromUri] string message)
        {
            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);

            return RenderLoginPage(message);
        }

        [Route("login")]
        public IHttpActionResult Post([FromUri] string message, Credentials model)
        {
            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey); 
            
            if (model == null)
            {
                return RenderLoginPage(message, "Invalid Username or Password");
            }

            if (!ModelState.IsValid)
            {
                var error =
                    from item in ModelState
                    where item.Value.Errors.Any()
                    from err in item.Value.Errors
                    select err.ErrorMessage;
                return RenderLoginPage(message, error.First(), model.Username);
            }

            var authResult = userService.Authenticate(model.Username, model.Password);
            if (authResult == null)
            {
                return RenderLoginPage(message, "Invalid Username or Password", model.Username);
            }

            var principal = IdentityServerPrincipal.Create(
                authResult.Subject,
                Constants.AuthenticationMethods.Password,
                Constants.BuiltInIdentityProvider);
            
            var id = principal.Identities.First();
            id.AddClaim(new Claim(ClaimTypes.Name, authResult.Username));

            Request.GetOwinContext().Authentication.SignIn(id);
            return Redirect(signInMessage.ReturnUrl);
        }

        [Route("logout")]
        [HttpGet]
        public IHttpActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut(Constants.BuiltInAuthenticationType);

            return new EmbeddedHtmlResult(Request,
                   new LayoutModel
                   {
                       Title = _settings.GetSiteName(),
                       Page = "logout"
                   });
        }

        [Route("external", Name="external")]
        [HttpGet]
        public IHttpActionResult SigninExternal(string provider, string message)
        {
            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey); 
            
            var ctx = Request.GetOwinContext();
            var authProp = new AuthenticationProperties {
                RedirectUri = Url.Route("callback", null)
            };
            authProp.Dictionary.Add("returnUrl", signInMessage.ReturnUrl);
            Request.GetOwinContext().Authentication.Challenge(authProp, provider);
            return Unauthorized();
        }

        [Route("callback", Name = "callback")]
        [HttpGet]
        public async Task<IHttpActionResult> ExternalCallback()
        {
            var ctx = Request.GetOwinContext();
            var authResult = await ctx.Authentication.AuthenticateAsync("idsrv.external");
            if (authResult == null)
            {
                return RedirectToRoute("login", null);
            }

            var principal = IdentityServerPrincipal.Create(
               authResult.Identity.Name,
               Constants.AuthenticationMethods.Password,
               Constants.BuiltInIdentityProvider);

            var id = principal.Identities.First();
            id.AddClaim(new Claim(ClaimTypes.Name, authResult.Identity.Name));
            
            ctx.Authentication.SignIn(id);

            var url = authResult.Properties.Dictionary["returnUrl"];
            return Redirect(url);
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


        private IHttpActionResult RenderLoginPage(string authorizeRequestMessage, string errorMessage = null, string username = null)
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
                        url = Request.RequestUri.AbsoluteUri,
                        username = username,
                        providers = new[] { 
                            new {
                                name="Google", 
                                url=Url.Route("external", new{provider="Google", message=authorizeRequestMessage}) 
                            }
                        }
                    }
                });
        }
    }
}
