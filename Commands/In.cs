namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

public class In : ICommand {
	public string command {
		get { return "in"; }
	}

	public string name {
		get { return "Delayed execution"; }
	}

	public IEnumerable<string> help {
		get {
			return new string[] {
				"in [secs] [command]",
				"",
				"Executes the specified command in the specified number of seconds."
			};
		}
	}

	public IEnumerable<string> execute(Models.UserState userState, string input, string[] parameters) {
		// Pull the time out.
		int time = int.Parse(parameters[1]);

		// Generate a new command line.
		List<string> newParams = new List<string>(parameters);
		newParams.RemoveAt(0);
		newParams.RemoveAt(0);
		string newInput = string.Join(" ", newParams.ToArray());

		// Execute the task delayed by the requested time.
		System.Threading.ThreadPool.QueueUserWorkItem((o) => {
			Thread.Sleep(1000 * time);
			var results = CommandController.Execute(userState, newInput);
			userState.outputPush(results);
		});
		return new string[0];
	}
}

}
