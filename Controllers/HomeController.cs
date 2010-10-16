namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

[HandleError]
public class HomeController : Controller {
	public ActionResult Index() {
		/* ViewData["Message"] = "Welcome to ASP.NET MVC!";

		return View(); */
		return RedirectToAction("Index", "Game");
	}

	public ActionResult About() {
		return View();
	}
}

}
