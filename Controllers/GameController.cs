namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kayateia.Climoo.Models;

/// <summary>
/// Controls the main game screen / loop.
/// </summary>
public class GameController : Controller {
	public ActionResult Index() {
		// Initialize the user's session if it's not there already.
		if (Session[UserState.SessionID] == null)
			Session[UserState.SessionID] = new UserState();

		return View("Console");
	}

	public JsonResult PushCheck() {
		var result = new ConsoleCommand() {
			resultText = "push."
		};

		System.Threading.Thread.Sleep(500);
		return Json(result, JsonRequestBehavior.AllowGet);
	}

	public JsonResult ExecCommand(string cmd) {
		var result = new Models.ConsoleCommand() {
			resultText = "Hey, you typed: " + cmd + "!"
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}
}

}
