namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserState {
	static public string SessionID = "UserState";

	public UserState() {
	}

	/// <summary>
	/// Adds a single line of output to the stack.
	/// </summary>
	public void outputPush(string line) {
		outputPush(new string[] { line });
	}

	/// <summary>
	/// Adds one or more lines of output to the stack.
	/// </summary>
	public void outputPush(IEnumerable<string> lines) {
		lock (_mutex) {
			_pendingOutput.AddRange(lines);
			_pendingOutputEvent.Set();
		}
	}

	/// <summary>
	/// Removes all lines of output from the stack, and returns them.
	/// </summary>
	public IEnumerable<string> outputPopAll() {
		lock (_mutex) {
			if (_pendingOutput.Count == 0)
				return new string[0];
			var rv = _pendingOutput;
			_pendingOutput = new List<string>();
			_pendingOutputEvent.Reset();
			return rv;
		}
	}

	/// <summary>
	/// Waits until some output is placed in the queue.
	/// </summary>
	public bool outputWait(int timeoutMillis) {
		return _pendingOutputEvent.WaitOne(timeoutMillis);
	}

	// Mutex which controls everything inside the state. This should always be
	// locked before using things in here.
	object _mutex = new object();

	// A list of strings representing pending console output lines.
	List<string> _pendingOutput = new List<string>();

	// This event is set when there is output waiting to be sent, and reset
	// when there is no output waiting.
	ManualResetEvent _pendingOutputEvent = new ManualResetEvent(false);
}

}