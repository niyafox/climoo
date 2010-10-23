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
		var player = _user.player;
		var playerLoc = Game.WorldData.world.findObject(player.locationId);

		var image = playerLoc.findAttribute(MooCore.Mob.Attributes.Image);
		string imageText = "";
		if (image != null)
			imageText = string.Format("<span style=\"float:right\"><img src=\"/Game/ServeAttribute?objectId={0}&attributeName={1}\" width=\"250\" /></span>",
				playerLoc.id,
				MooCore.Mob.Attributes.Image);

		string output = string.Format(@"
				<p><b>{0}</b> ({1}) {2}</p>
				<p>{3}</p>
			",
			playerLoc.name,
			playerLoc.fqpn,
			imageText,
			playerLoc.desc);

		var contents = playerLoc.contained.Where((m) => m.id != player.id);
		if (contents.Count() > 0) {
			output += "<p><b>Also here</b>: ";
			foreach (var m in contents)
				output += m.name + ", ";
			output = output.Substring(0, output.Length - 2) + "</p>";
		}

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

	public ActionResult ServeAttribute(int objectId, string attributeName) {
		MooCore.Mob mob = Game.WorldData.world.findObject(objectId);
		if (mob == null)
			return null;

		var attr = mob.findAttributeAndType(attributeName);
		return this.File(attr.contentsAsBytes, attr.mimetype);
	}
}

}
