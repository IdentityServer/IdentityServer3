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

        //public ActionResult CallService()
        //{

        //}
	}
}