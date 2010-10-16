namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Kayateia.Climoo.Models;

/// <summary>
/// Controls the main game screen / loop.
/// </summary>
public class GameController : AsyncController {
	// The actual main page view.
	public ActionResult Index() {
		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output. It's sort of silly that we have to do this async
	// pattern for this, but it's not terrible.
	public void PushCheckAsync() {
		AsyncManager.OutstandingOperations.Increment();

		// Pull the piece of the session we care about.
		UserState state = _state;

		// Execute this in the background.
		ThreadPool.QueueUserWorkItem((o) => {
			// Wait for new output, and fail if we don't get any by 25 seconds.
			IEnumerable<string> rv;
			if (!state.outputWait(25000))
				rv = new string[0];
			else {
				// Get what's there, if anything is left.
				rv = state.outputPopAll();
			}

			// Sync back up with ASP.
			AsyncManager.Sync(() => {
				AsyncManager.Parameters["newLines"] = rv;
				AsyncManager.OutstandingOperations.Decrement();
			});
		});
	}
	public JsonResult PushCheckCompleted(IEnumerable<string> newLines) {
		var result = new ConsoleCommand() {
			resultText = string.Join("<br/>", newLines)
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Called by the page when the user types a command. This may return
	// data immediately rather than waiting for the push.
	public JsonResult ExecCommand(string cmd) {
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
