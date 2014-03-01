using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcFormPostClient.Controllers
{
	public class AccountController : Controller
	{
		public ActionResult SignIn()
		{
			return View();
		}

		public ActionResult SignInCallback()
		{
			return View();
		}

		public ActionResult SignOut()
		{
			return View();
		}
	}
}