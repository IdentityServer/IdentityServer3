using Owin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityModel.Extensions;
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
        //IAuthenticationService authenticationService;
        //public LoginController(IAuthenticationService authenticationService)
        //{
        //    this.authenticationService = authenticationService;
        //}

        IUserService userService;

        public LoginController(IUserService userService)
        {
            this.userService = userService;
        }

        [Route("login")]
        public HttpResponseMessage Get([FromUri] SignInMessage msg)
        {
            var html = EmbeddedResourceManager.LoadResourceString("Thinktecture.IdentityServer.Core.Authentication.Assets.Login.html");
            html = html.Replace("{title}", "IdSrv3 Login");
            html = html.Replace("{loginurl}", Request.RequestUri.AbsoluteUri);
            if (msg != null)
            {
                html = html.Replace("{returnUrl}", msg.ReturnUrl);
            }

            return new HttpResponseMessage()
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
        }

        [Route("signin")]
        public IHttpActionResult Post(Credentials model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "Invalid Username or Password");
            }

            if (!ModelState.IsValid)
            {
                var error =
                    from item in ModelState
                    where item.Value.Errors.Any()
                    from err in item.Value.Errors
                    select err.ErrorMessage;
                return BadRequest(error.First());
            }

            var sub = userService.Authenticate(model.Username, model.Password);
            if (sub == null)
            {
                return BadRequest("Invalid Username or Password");
            }

            //UserAccount acct;
            //if (!userAccountService.Authenticate(model.Username, model.Password, out acct))
            //{
            //    return BadRequest("Invalid Username or Password");
            //}

            //var claims = acct.GetAllClaims();
            //var ci = new ClaimsIdentity(claims, "Cookie");

            //var ctx = Request.GetOwinContext();
            //ctx.Authentication.SignIn(ci);

            //authenticationService.SignIn(acct);
            var identity = new ClaimsIdentity("idsrv");
            identity.AddClaim(new Claim(Constants.ClaimTypes.Subject, sub));
            identity.AddClaim(new Claim(Constants.ClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString()));
            identity.AddClaim(new Claim(Constants.ClaimTypes.AuthenticationMethod, Constants.AuthenticationMethods.Password));
            Request.GetOwinContext().Authentication.SignIn(identity);

            return Content(HttpStatusCode.OK, new
            {
                id = sub,
            });
        }

        [Route("signout")]
        public HttpResponseMessage Delete()
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
