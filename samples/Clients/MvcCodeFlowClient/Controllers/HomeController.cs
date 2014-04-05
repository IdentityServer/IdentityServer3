using System.IdentityModel.Services;
using System.Security.Claims;
using System.Web.Mvc;

namespace CodeFlowClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }
        
        public ActionResult OidcError(string error)
        {
            ViewBag.Message = error;

            return View();
        }
        
        public ActionResult Logout()
        {
            FederatedAuthentication.SessionAuthenticationModule.SignOut();
            return Redirect("~/");
        }

        [Authorize]
        public ActionResult Claims()
        {
            ViewBag.Message = "Claims:";

            return View(ClaimsPrincipal.Current);
        }
    }
}
