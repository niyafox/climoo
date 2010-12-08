namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class PublicSite : UITask {
	public PublicSite(Session.UserContext context) : base(context) {
		_commands = new Dictionary<StringI,Func<IEnumerable<Result>>> {
			{ "login", this.login },
			{ "help", () => { return showScreen("help"); } },
			{ "about", () => { return showScreen("about"); } }
		};
	}

	Dictionary<StringI, Func<IEnumerable<Result>>> _commands;

	protected override IEnumerable<Result> runTask() {
		showWelcome();

		for ( ; ; ) {
			yield return Result.GetInput();

			Func<IEnumerable<Result>> handler = null;
			string[] pieces = _input.Split(' ', '\t');
			if (pieces.Length > 0)
				handler = getCommand(pieces[0]);

			if (handler == null) {
				_context.outputPush("Sorry, don't know what that means.");
				showWelcome();
				continue;
			}

			foreach (var i in handler()) yield return i;
		}
	}

	IEnumerable<Result> login() {
		string[] pieces = _input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		string username;
		if (pieces.Length < 2) {
			_context.outputPush("&gt;&gt; Login: ");
			yield return Result.GetInput();
			username = _input;
		} else
			username = pieces[1];

		string password;
		if (pieces.Length < 3) {
			_context.outputPush("&gt;&gt;Password: ");
			yield return Result.GetInput();
			password = _input;
		} else
			password = pieces[2];

		string result = Game.Login.LogUserIn(_context, username, password);
		if (result != null) {
			_context.outputPush("Could not log you in: " + result);
			yield break;
		}

		_context.outputPush("\nSuddenly you're falling...!\n\n");
		MooCore.InputParser.ProcessInput("look", _context.player);
		yield return Result.ToGame();
	}

	Func<IEnumerable<Result>> getCommand(string cmd) {
		return (from c in _commands where c.Key == cmd select c.Value).FirstOrDefault();
	}

	IEnumerable<Result> showScreen(string screen) {
		_context.outputPush(Models.Screens.Get(screen));
		yield break;
	}

	void showWelcome() {
		string welcome = Models.Screens.Get("welcome");
		welcome += "\nCommands: " + String.Join(" ", _commands.Keys);
		_context.outputPush(welcome);
	}
}

}
