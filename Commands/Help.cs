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
		if (parameters.Length == 1) {
			List<string> outp = new List<string>();
			outp.Add("The following commands are available:");
			foreach (var c in CommandController.GetAll())
				outp.Add(string.Format("{0} - {1}", c.command, c.name));
			return outp;
		} else
			cmd = CommandController.LookUp(parameters[1]);

		// Print out the help text.
		return cmd.help;
	}
}

}
