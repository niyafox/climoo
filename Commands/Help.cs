namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Help : ICommand {
	public string command {
		get { return "help"; }
	}

	public string name {
		get { return "View help on a command"; }
	}

	public IEnumerable<string> help {
		get {
			return new string[] {
				"help [command]",
				"",
				"Shows the help for the specified command."
			};
		}
	}

	public IEnumerable<string> execute(Session.UserContext userState, string input, string[] parameters) {
		// Find the requested command, defaulting to us.
		ICommand cmd;
		if (parameters.Length == 1)
			cmd = this;
		else
			cmd = CommandController.LookUp(parameters[1]);

		// Print out the help text.
		return cmd.help;
	}
}

}
