using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Core.Plumbing;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [RoutePrefix("login")]
    [Route]
    public class LoginController : ApiController
    {
        string GetLoginPage()
        {
            var html = EmbeddedResourceManager.LoadResourceString("Thinktecture.IdentityServer.Core.Authentication.Assets.Login.html");
            html = html.Replace("{title}", "IdSrv3 Login");
            html = html.Replace("{loginurl}", Request.RequestUri.AbsoluteUri);
            return html;
        }
        HttpResponseMessage GetLoginPageResponse()
        {
            var html = GetLoginPage();
            return new HttpResponseMessage()
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };
        }

        public HttpResponseMessage Get([FromUri] SignInMessage msg)
        {
            return GetLoginPageResponse();
        }

        public IHttpActionResult Post([FromUri] SignInMessage msg)
        {
            var message = Request.RequestUri.Query;

            var id = new ClaimsIdentity("idsrv");
            id.AddClaims(new[]
            {
                new Claim(Constants.ClaimTypes.Subject, "dominick"),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, Constants.AuthenticationMethods.Password),
                new Claim(Constants.ClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString())
            });

            Request.GetOwinContext().Authentication.SignIn(id);

            return Redirect(msg.ReturnUrl);
        }
    }
}
