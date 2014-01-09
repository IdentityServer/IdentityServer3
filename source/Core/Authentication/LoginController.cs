using System;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    [RoutePrefix("login")]
    public class LoginController : ApiController
    {
        [Route]
        public IHttpActionResult Get()
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

            return Redirect(Request.RequestUri.ParseQueryString().Get("returnUrl"));
        }
    }
}
