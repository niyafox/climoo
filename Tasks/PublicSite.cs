namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class PublicSite : UITask {
	public PublicSite(Session.UserContext context) : base(context) {
	}

	protected override IEnumerable<Result> runTask() {
		showWelcome();

		for ( ; ; ) {
			yield return Result.GetInput();

			if (_input.Length == 0) {
				showWelcome();
				continue;
			}

			if (!_input.StartsWithI("login ")) {
				_context.outputPush("Sorry, only 'login' is allowed here.");
				continue;
			}

			string username = _input.Substring("login ".Length);
			string result = Game.Login.LogUserIn(_context, username, "");
			if (result != null) {
				_context.outputPush("Could not log you in: " + result);
				continue;
			}

			_context.outputPush("\nSuddenly you're falling...!\n\n");
			MooCore.InputParser.ProcessInput("look", _context.player);
			yield return Result.ToGame();
		}
	}

	void showWelcome() {
		string welcome = Models.Screens.Get("welcome");
		welcome += "\n"
			+ "Commands: login";
		_context.outputPush(welcome);
	}
}

}
