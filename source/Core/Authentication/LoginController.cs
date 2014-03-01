using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
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

        [Route("login")]
        public IHttpActionResult Get([FromUri] string message)
        {
            var protection = _settings.GetInternalProtectionSettings();
            var signInMessage = SignInMessage.FromJwt(
                message,
                protection.Issuer,
                protection.Audience,
                protection.SigningKey);
            
            return new EmbeddedHtmlResult(
                Request,
                new LayoutModel { 
                    Title=_settings.GetSiteName(),
                    Page = "login", 
                    PageModel = new { url=Request.RequestUri.AbsoluteUri } 
                });
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
                return new EmbeddedHtmlResult(Request,
                    new LayoutModel
                    {
                        Title = _settings.GetSiteName(),
                        Page = "login",
                        PageModel = new { url = Request.RequestUri.AbsoluteUri },
                        ErrorMessage = "Invalid Username or Password",
                    });
            }

            if (!ModelState.IsValid)
            {
                var error =
                    from item in ModelState
                    where item.Value.Errors.Any()
                    from err in item.Value.Errors
                    select err.ErrorMessage;
                return new EmbeddedHtmlResult(Request,
                    new LayoutModel
                    {
                        Title = _settings.GetSiteName(),
                        Page = "login",
                        PageModel = new { url = Request.RequestUri.AbsoluteUri, username=model.Username },
                        ErrorMessage = error.First(),
                    });
            }

            var sub = userService.Authenticate(model.Username, model.Password);
            if (sub == null)
            {
                return new EmbeddedHtmlResult(Request,
                    new LayoutModel
                    {
                        Title = _settings.GetSiteName(),
                        Page = "login",
                        PageModel = new { url = Request.RequestUri.AbsoluteUri, username = model.Username },
                        ErrorMessage = "Invalid Username or Password",
                    });
            }

            var principal = IdentityServerPrincipal.Create(
                sub,
                Constants.AuthenticationMethods.Password);

            Request.GetOwinContext().Authentication.SignIn(principal.Identities.First());
            return Redirect(signInMessage.ReturnUrl);
        }

        [Route("logout")]
        public HttpResponseMessage Post()
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut("Cookie");

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        //public IHttpActionResult Post([FromUri] SignInMessage msg, [FromBody] LoginBody body)
        //{
        //    var claims = authenticationService.Authenticate(body.Username, body.Password);
        //    if (claims != null)
        //    {
        //        var message = Request.RequestUri.Query;

        //        var id = new ClaimsIdentity("idsrv");
        //        id.AddClaims(new[]
        //        {
        //            new Claim(Constants.ClaimTypes.Subject, body.Username),
        //            new Claim(Constants.ClaimTypes.AuthenticationMethod, Constants.AuthenticationMethods.Password),
        //            new Claim(Constants.ClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString())
        //        });
        //        id.AddClaims(claims);

        //        Request.GetOwinContext().Authentication.SignIn(id);

        //        return Redirect(msg.ReturnUrl);
        //    }

        //    return ResponseMessage(GetLoginPageResponse());
        //}


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

        //[Route("external", Name = "SigninExternal")]
        //[HttpGet]
        //public IHttpActionResult SigninExternal(string provider)
        //{
        //    var authProp = new AuthenticationProperties { RedirectUri = Url.Link("ExternalCallback", new { provider }) };
        //    Request.GetOwinContext().Authentication.Challenge(authProp, provider);
        //    return Unauthorized();
        //}

        //[Route("callback", Name = "ExternalCallback")]
        //[HttpGet]
        //[HostAuthentication("ExternalCookie")]
        //public IHttpActionResult ExternalCallback(string provider)
        //{
        //    var ctx = Request.GetOwinContext();
        //    ctx.Authentication.SignOut("ExternalCookie");

        //    var cp = User as ClaimsPrincipal;
        //    var id = cp.Claims.GetValue(ClaimTypes.NameIdentifier);
        //    var otherClaims = cp.Claims.Where(x => x.Type != ClaimTypes.NameIdentifier);

        //    // what to do?
        //    // 1) interactive
        //    // => redirect to some page so they can build the local account
        //    // 2) silent
        //    // => auto create account (via extensibility point) 
        //    try
        //    {
        //        this.AuthenticationService.SignInWithLinkedAccount(provider, id, otherClaims);
        //    }
        //    catch (ValidationException ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return BadRequest(ModelState);
        //    }

        //    return Redirect("~/");
        //}
    }
}
