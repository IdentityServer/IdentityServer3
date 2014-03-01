using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Client;

namespace MvcFormPostClient.Controllers
{
	public class AccountController : Controller
	{
		public ActionResult SignIn()
		{
			var client = new OAuth2Client(new Uri(Constants.AuthorizeEndpoint));
			
            var url = client.CreateAuthorizeUrl(
				"formpostclient",
				"id_token",
				"openid email",
				"http://localhost:11716/account/signInCallback",
				"state",
				new Dictionary<string, string>
				{
					{ "nonce", "nonce" },
					{ "response_mode", "form_post" }
				});
				
            return Redirect(url);
		}

        [HttpPost]
		public ActionResult SignInCallback()
		{
            var token = Request.Form["id_token"];
            var state = Request.Form["state"];

            var claims = ValidateIdentityToken(token);

            var id = new ClaimsIdentity(claims, "Cookies");
            Request.GetOwinContext().Authentication.SignIn(id);

            return Redirect("/");
		}

        private IEnumerable<Claim> ValidateIdentityToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                AllowedAudience = "formpostclient",
                ValidIssuer = "https://idsrv3.com",
                SigningToken = new X509SecurityToken(X509.LocalMachine.My.SubjectDistinguishedName.Find("CN=sts", false).First())
            };

            var id = new JwtSecurityTokenHandler().ValidateToken(token, parameters);
            return id.Claims;
        }

		public ActionResult SignOut()
		{
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect("/");
		}
	}
}