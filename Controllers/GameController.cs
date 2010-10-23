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
		int curRoom = 2;
		var mob = Models.WorldData.world.findObject(curRoom);
		string output = string.Format(@"
			<p><b>{0}</b></p>
			<p>{1}</p>
		",
		mob.attributes[MooCore.Mob.Attributes.Name],
		mob.attributes[MooCore.Mob.Attributes.Description]);
		_user.outputPush(output);

		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output.
	public JsonResult PushCheck() {
		// Wait for new output, and fail if we don't get any by 25 seconds.
		IEnumerable<string> newLines;
		if (!_user.outputWait(25000))
			newLines = new string[0];
		else {
			// Get what's there, if anything is left.
			newLines = _user.outputPopAll();
		}

		var result = new ConsoleCommand() {
			resultText = string.Join("<br/>", newLines)
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Called by the page when the user types a command. This may return
	// data immediately rather than waiting for the push.
	public JsonResult ExecCommand(string cmd) {
		Trace.WriteLine("Executing command");
		var result = new Models.ConsoleCommand() {
			resultText = string.Join("<br/>", Commands.CommandController.Execute(_user, cmd).ToArray())
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}
}

}
