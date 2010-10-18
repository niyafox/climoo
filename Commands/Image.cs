namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Image : ICommand {
	public string command {
		get { return "image"; }
	}

	public string name {
		get { return "Shows an image"; }
	}

	public IEnumerable<string> help {
		get { return new string[] { "There's no helping you" }; }
	}

	public IEnumerable<string> execute(Session.UserContext userState, string input, string[] parameters) {
		return new string[] {
			string.Format("<img src=\"/Content/{0}\" />", parameters[1])
		};
	}
}

}
