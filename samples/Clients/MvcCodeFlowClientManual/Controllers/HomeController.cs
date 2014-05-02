using Sample;
using System;
using System.Web.Mvc;
using System.Web;
using Thinktecture.IdentityModel.Client;

namespace MvcCodeFlowClientManual.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Request.GetOwinContext().Authentication.SignOut("Cookies");

            return View();
        }

        [HttpPost]
        public ActionResult Index(string scopes)
        {
            var client = new OAuth2Client(new Uri(Constants.AuthorizeEndpoint));
            var url = client.CreateCodeFlowUrl(
                "codeclient",
                scopes,
                "https://localhost:44312/callback",
                "123");

            return Redirect(url);
        }
    }
}