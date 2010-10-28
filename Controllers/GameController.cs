namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Trace = System.Diagnostics.Trace;
using Kayateia.Climoo.Models;

/// <summary>
/// Controls the main game screen / loop.
/// </summary>
public class GameController : Session.SessionFreeController {
	// The actual main page view.
	public ActionResult Index() {
		MooCore.InputParser.ProcessInput("look", _user.player);
		/* var contents = playerLoc.contained.Where((m) => m.id != player.avatar.id);
		if (contents.Count() > 0) {
			output += "<p><b>Also here</b>: ";
			foreach (var m in contents)
				output += m.name + ", ";
			output = output.Substring(0, output.Length - 2) + "</p>";
		}

		_user.outputPush(output); */

		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output.
	public JsonResult PushCheck() {
		// Wait for new output, and fail if we don't get any by 25 seconds.
		string newText;
		if (!_user.outputWait(25000))
			newText = "";
		else {
			// Get what's there, if anything is left.
			newText = _user.outputPopAll();
		}

		var result = new ConsoleCommand() {
			resultText = newText
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Called by the page when the user types a command. This may return
	// data immediately rather than waiting for the push.
	public JsonResult ExecCommand(string cmd) {
		Trace.WriteLine("Executing command");
		var result = new Models.ConsoleCommand() {
			resultText = MooCore.InputParser.ProcessInput(cmd, _user.player)
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	public ActionResult ServeAttribute(int objectId, string attributeName) {
		MooCore.Mob mob = Game.WorldData.world.findObject(objectId);
		if (mob == null)
			return null;

		var attr = mob.findAttributeAndType(attributeName);
		return this.File(attr.contentsAsBytes, attr.mimetype);
	}
}

}
