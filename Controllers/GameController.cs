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
public class GameController : Controller {
	// The actual main page view.
	public ActionResult Index() {
		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output. It's sort of silly that we have to do this async
	// pattern for this, but it's not terrible.
	public JsonResult PushCheck() {
		// Pull the piece of the session we care about.
		UserState state = _state;

		// Wait for new output, and fail if we don't get any by 25 seconds.
		IEnumerable<string> newLines;
		if (!state.outputWait(25000))
			newLines = new string[0];
		else {
			// Get what's there, if anything is left.
			newLines = state.outputPopAll();
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
			resultText = string.Join("<br/>", Commands.CommandController.Execute(_state, cmd).ToArray())
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Retrieve the user's state, or make a new one.
	private UserState _state {
		get {
			if (_stateCache == null) {
				// Initialize the user's session if it's not there already.
				if (Session[UserState.SessionID] == null)
					Session[UserState.SessionID] = new UserState();
				_stateCache = (UserState)Session[UserState.SessionID];
			}
			return _stateCache;
		}
	}
	private UserState _stateCache = null;
}

}
