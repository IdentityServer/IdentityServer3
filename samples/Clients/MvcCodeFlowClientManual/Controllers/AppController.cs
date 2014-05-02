using Newtonsoft.Json.Linq;
using Sample;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

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
	}
}