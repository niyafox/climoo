namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

public class CommandController {
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

	static public IEnumerable<string> Execute(Models.UserState state, string input) {
		// Make the "parameter" array we'll pass in.
		string[] pieces = input.Split(' ', '\t', '\n');

		// Look for a command matching the first bit.
		if (!s_commands.ContainsKey(pieces[0]))
			return new string[] { string.Format("Unknown command '{0}'.", pieces[0]) };

		// Pass the request off to the command in question.
		return s_commands[pieces[0]].execute(state, input, pieces);
	}
}

}
