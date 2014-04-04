using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;

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

        private string ParseJwt(string token)
        {
            if(!token.Contains("."))
            {
                return token;
            }

            var parts = token.Split('.');
            var part = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));

            var jwt = JObject.Parse(part);
            return jwt.ToString();
        }
	}
}