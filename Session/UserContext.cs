namespace Kayateia.Climoo.Session {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserContext : IDisposable {
	public UserContext() {
		var world = Game.WorldData.world;
		this.player = new MooCore.Player(world.createObject(new {
				name = "Kayateia",
				desc = "With something approaching Polynesian looks, slightly pointy ears, "
					+ "and a definite mischievous twinkle in her eyes, this girl gives you "
					+ "thoughts of fae."
			},
			location: world.findObject(":entry").id,
			parent: world.findObject(":templates:player").id));
	}

	public void Dispose() {
		Game.WorldData.world.destroyObject(this.player.avatar.id);
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
			use(true);
			_pendingOutput.AddRange(lines);
			_pendingOutputEvent.Set();
		}
	}

	/// <summary>
	/// Removes all lines of output from the stack, and returns them.
	/// </summary>
	public IEnumerable<string> outputPopAll() {
		lock (_mutex) {
			use(true);
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
		use(false);
		return _pendingOutputEvent.WaitOne(timeoutMillis);
	}

	/// <summary>
	/// Get/Set the player world-object within the MOO.
	/// </summary>
	// This isn't particularly locked because it's not really going to be multi-set.
	public MooCore.Player player {
		get { return _player; }
		set {
			_player = value;
			_player.NewOutput += (text) => {
				outputPush(string.Format("<p>{0}</p>", text));
			};
		}
	}
	MooCore.Player _player;

	// Call from the other methods any time we're used.
	void use(bool insideMutex) {
		if (!insideMutex) {
			lock (_mutex)
				_lastUse = DateTimeOffset.UtcNow;
		} else
			_lastUse = DateTimeOffset.UtcNow;
	}

	// Mutex which controls everything inside the state. This should always be
	// locked before using things in here.
	object _mutex = new object();

	// A list of strings representing pending console output lines.
	List<string> _pendingOutput = new List<string>();

	// This event is set when there is output waiting to be sent, and reset
	// when there is no output waiting.
	ManualResetEvent _pendingOutputEvent = new ManualResetEvent(false);

	// Last use time, for garbage collection.
	DateTimeOffset _lastUse = DateTimeOffset.UtcNow;
}

}