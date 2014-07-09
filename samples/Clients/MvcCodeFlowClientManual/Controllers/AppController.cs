using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;
using System.Linq;
using System.Web;

namespace MvcCodeFlowClientManual.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> CallService()
        {
            var principal = User as ClaimsPrincipal;

            var client = new HttpClient();
            client.SetBearerToken(principal.FindFirst("access_token").Value);

            var result = await client.GetStringAsync(Constants.AspNetWebApiSampleApi + "identity");

            return View(JArray.Parse(result));
        }

        public async Task<ActionResult> RefreshToken()
        {
            var client = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "codeclient",
                "secret");

            var principal = User as ClaimsPrincipal;
            var refreshToken = principal.FindFirst("refresh_token").Value;

            var response = await client.RequestRefreshTokenAsync(refreshToken);
            UpdateCookie(response);

            return RedirectToAction("Index");
        }

        private void UpdateCookie(TokenResponse response)
        {
            var identity = (User as ClaimsPrincipal).Identities.First();
            var result = from c in identity.Claims
                         where c.Type != "access_token" &&
                               c.Type != "refresh_token" &&
                               c.Type != "expires_at"
                         select c;

            var claims = result.ToList();

            claims.Add(new Claim("access_token", response.AccessToken));
            claims.Add(new Claim("expires_at", (DateTime.UtcNow.ToEpochTime() + response.ExpiresIn).ToDateTimeFromEpoch().ToString()));
            claims.Add(new Claim("refresh_token", response.RefreshToken));
            
            var newId = new ClaimsIdentity(claims, "Cookies");
            Request.GetOwinContext().Authentication.SignIn(newId);
        }
	}
}