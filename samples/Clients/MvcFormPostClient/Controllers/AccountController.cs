using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            var client = new OAuth2Client(new Uri(Constants.AuthorizeEndpoint));

            var url = client.CreateAuthorizeUrl(
                "implicitclient",
                "id_token",
                "openid email",
                "http://localhost:11716/account/signInCallback",
                state,
                new Dictionary<string, string>
				{
					{ "nonce", nonce },
					{ "response_mode", "form_post" }
				});

            SetTempCookie(state, nonce);
            return Redirect(url);
        }

        [HttpPost]
        public async Task<ActionResult> SignInCallback()
        {
            var token = Request.Form["id_token"];
            var state = Request.Form["state"];

            var claims = await ValidateIdentityTokenAsync(token, state);

            var id = new ClaimsIdentity(claims, "Cookies");
            Request.GetOwinContext().Authentication.SignIn(id);

            return Redirect("/");
        }

        private async Task<IEnumerable<Claim>> ValidateIdentityTokenAsync(string token, string state)
        {
            var result = await Request.GetOwinContext().Authentication.AuthenticateAsync("TempCookie");
            if (result == null)
            {
                throw new InvalidOperationException("No temp cookie");
            }

            if (state != result.Identity.FindFirst("state").Value)
            {
                throw new InvalidOperationException("invalid state");
            }

            var parameters = new TokenValidationParameters
            {
                AllowedAudience = "implicitclient",
                ValidIssuer = "https://idsrv3.com",
                SigningToken = new X509SecurityToken(X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find("CN=idsrv3test", false).First())
            };

            var id = new JwtSecurityTokenHandler().ValidateToken(token, parameters);

            if (id.FindFirst("nonce").Value != result.Identity.FindFirst("nonce").Value)
            {
                throw new InvalidOperationException("Invalid nonce");
            }

            Request.GetOwinContext().Authentication.SignOut("TempCookie");
            return id.Claims;
        }

        public ActionResult SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return Redirect(Constants.LogoutEndpoint);
        }

        private void SetTempCookie(string state, string nonce)
        {
            var tempId = new ClaimsIdentity("TempCookie");
            tempId.AddClaim(new Claim("state", state));
            tempId.AddClaim(new Claim("nonce", nonce));

            Request.GetOwinContext().Authentication.SignIn(tempId);
        }
    }
}