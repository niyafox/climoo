namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

public class CommandController {
	/// <summary>
	/// Bundle of info needed to execute a command.
	/// </summary>
	public struct Action {
		/// <summary>
		/// The handler located to execute the command. This may be null
		/// if no handler was found.
		/// </summary>
		public ICommand handler;

		/// <summary>
		/// The original input string.
		/// </summary>
		public string input;

		/// <summary>
		/// The original input string, broken into pieces.
		/// </summary>
		public string[] inputPieces;
	}

	static CommandController() {
		s_commands = new Dictionary<string,ICommand>();

		// Scan for classes that implement ICommand, and sequester copies here.
		Type[] allTypes = typeof(CommandController).Assembly.GetTypes();
		foreach (Type t in allTypes) {
			if (t.GetInterfaces().Contains(typeof(ICommand))) {
				ICommand cmd = (ICommand)Activator.CreateInstance(t);
				s_commands[cmd.command]= cmd;
			}
		}
	}
	static Dictionary<string, ICommand> s_commands;

	static public IEnumerable<ICommand> GetAll() {
		return s_commands.Values;
	}

	static public ICommand LookUp(string cmd) {
		if (s_commands.ContainsKey(cmd))
			return s_commands[cmd];
		else
			return null;
	}

	static public Action GetPlan(string input) {
		// Make the "parameter" array we'll pass in.
		string[] pieces = input.Split(' ', '\t', '\n');

		// Look for a command matching the first bit.
		ICommand cmd = null;
		if (s_commands.ContainsKey(pieces[0]))
			cmd = s_commands[pieces[0]];

		return new Action() {
			handler = cmd,
			input = input,
			inputPieces = pieces
		};
	}

	static public IEnumerable<string> Execute(Session.UserContext state, string input) {
		Action act = GetPlan(input);
		if (act.handler == null) {
			// Couldn't find a way.
			return new string[] { string.Format("Unknown command '{0}'.", act.inputPieces[0]) };
		} else {
			try {
				// Pass the request off to the command in question.
				return act.handler.execute(state, act.input, act.inputPieces);
			} catch (System.Exception exc) {
				List<string> rv = new List<string>();
				rv.Add("Unable to execute request:");
				rv.Add("");
				rv.AddRange(exc.ToString().Split('\n'));
				return rv;
			}
		}
	}
}

}
