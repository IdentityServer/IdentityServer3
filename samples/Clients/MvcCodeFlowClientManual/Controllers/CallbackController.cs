using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Client;
using System.Linq;
using System.Web;

namespace MvcCodeFlowClientManual.Controllers
{
	public class CallbackController : Controller
	{
		public ActionResult Index()
		{
            ViewBag.Code = Request.QueryString["code"] ?? "none";
            ViewBag.Error = Request.QueryString["error"] ?? "none";

			return View();
		}

        [HttpPost]
        [ActionName("Index")]
        public async Task<ActionResult> GetToken()
        {
            var client = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "codeclient",
                "secret");

            var code = Request.QueryString["code"];

            var response = await client.RequestAuthorizationCodeAsync(
                code,
                "https://localhost:44312/callback");

            ValidateResponseAndSignIn(response);

            if (!string.IsNullOrEmpty(response.IdentityToken))
            {
                ViewBag.IdentityTokenParsed = ParseJwt(response.IdentityToken);
            }
            if (!string.IsNullOrEmpty(response.AccessToken))
            {
                ViewBag.AccessTokenParsed = ParseJwt(response.AccessToken);
            }

            return View("Token", response);
        }

        private void ValidateResponseAndSignIn(TokenResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.IdentityToken))
            {
                var claims = ValidateToken(response.IdentityToken);

                if (!string.IsNullOrWhiteSpace(response.AccessToken))
                {
                    claims.Add(new Claim("access_token", response.AccessToken));
                    claims.Add(new Claim("expires_at", (DateTime.UtcNow.ToEpochTime() + response.ExpiresIn).ToDateTimeFromEpoch().ToString()));
                }

                if (!string.IsNullOrWhiteSpace(response.RefreshToken))
                {
                    claims.Add(new Claim("refresh_token", response.RefreshToken));
                }

                var id = new ClaimsIdentity(claims, "Cookies");
                Request.GetOwinContext().Authentication.SignIn(id);
            }
        }

        private List<Claim> ValidateToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                AllowedAudience = "codeclient",
                ValidIssuer = "https://idsrv3.com",
                SigningToken = new X509SecurityToken(X509.LocalMachine.TrustedPeople.SubjectDistinguishedName.Find("CN=idsrv3test", false).First())
            };

            var id = new JwtSecurityTokenHandler().ValidateToken(token, parameters);
            return id.Claims.ToList();
        }

        private string ParseJwt(string token)
        {
            if(!token.Contains("."))
            {
                return token;
            }

            var parts = token.Split('.');
            var part = Encoding.UTF8.GetString(Thinktecture.IdentityModel.Base64Url.Decode(parts[1]));

            var jwt = JObject.Parse(part);
            return jwt.ToString();
        }
	}
}