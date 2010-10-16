namespace Kayateia.Climoo.Commands {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a command the user may execute from their shell.
/// </summary>
/// <remarks>
/// Instances of these classes may be reused, so don't plan on storing
/// any long-term state in them related to a single command run.
/// </remarks>
public interface ICommand {
	/// <summary>
	/// The actual string representing what the user types for this command.
	/// </summary>
	string command { get; }

	/// <summary>
	/// The name of this command, human-friendly.
	/// </summary>
	string name { get; }

	/// <summary>
	/// The help text for this command.
	/// </summary>
	IEnumerable<string> help { get; }

	/// <summary>
	/// Execute the command on the specified user context. May return some
	/// lines of text to be sent back to the user's console.
	/// </summary>
	IEnumerable<string> execute(Models.UserState userState, string input, string[] parameters);
}

}
