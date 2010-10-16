namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

public class GameController : Controller {
	public ActionResult Index() {
		return View("Console");
	}

	public JsonResult PushCheck() {
		var result = new Models.CommandModel() {
			resultText = "push."
		};

		System.Threading.Thread.Sleep(500);
		return Json(result, JsonRequestBehavior.AllowGet);
	}

	public JsonResult ExecCommand(string cmd) {
		var result = new Models.CommandModel() {
			resultText = "Hey, you typed: " + cmd + "!"
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}
}

}
