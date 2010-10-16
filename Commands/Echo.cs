namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Really simplistic proof of concept command.
/// </summary>
public class Echo : ICommand {
	#region ICommand impl
	public string command {
		get { return "echo"; }
	}

	public string name {
		get { return "Echo"; }
	}

	public IEnumerable<string> help {
		get {
			return new string[] {
				"echo [params]",
				"",
				"Prints [params] back to the console."
			};
		}
	}

	public IEnumerable<string> execute(Models.UserState userState, string input, string[] parameters) {
		// Presumably we can chop off "echo " and go from there.
		return new string[] {
			input.Substring(5)
		};
	}
	#endregion
}

}
