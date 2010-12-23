namespace Kayateia.Climoo.Session {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Text;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserContext : IDisposable {
	public UserContext() {
		var world = Game.WorldData.world;
		world.attributeUrlGenerator = (mob, attr) => {
			return string.Format("/Game/ServeAttribute?objectId={0}&attributeName={1}", mob.id, attr);
		};

		newTask(new Tasks.PublicSite(this));
	}

	public void Dispose() {
		Game.Login.LogUserOut(this);
		newTask(null);
	}

	/// <summary>
	/// Call when a new piece of input is received from the user.
	/// </summary>
	/// <param name="text">The new input</param>
	/// <returns>Some text to display to the user, if any.</returns>
	public string inputPush(string text) {
		use(false);

		if (_task != null) {
			var action = _task.inputPush(text);
			switch (action.action) {
			case Tasks.UITask.Action.NoAction:
				return "";
			case Tasks.UITask.Action.Output:
				return action.output;
			case Tasks.UITask.Action.NewTask:
				newTask(action.newTask, false);
				return "";
			case Tasks.UITask.Action.ToGame:
				newTask(null, false);
				return "";
			default:
				throw new InvalidOperationException("UITask.Action enum is not sync'd with UserContext.inputPush()");
			}
		} else {
			if (text == "logout") {
				outputPush("<br/>Goodbye!<br/><br/>");
				Game.Login.LogUserOut(this);
				newTask(new Tasks.PublicSite(this));
				return "";
			} else {
				try {
					return MooCore.InputParser.ProcessInput(text, this.player);
				} catch (System.Exception ex) {
					return "<span class=\"error\">Exception: {0}".FormatI(ex.Message);
				}
			}
		}
	}

	/// <summary>
	/// Switches to a new user interaction task. The old task will be stopped
	/// and removed first.
	/// </summary>
	public void newTask(Tasks.UITask task, bool stopOld = true) {
		if (_task != null && stopOld)
			_task.stop();
		_task = task;
		if (_task != null) {
			var action = _task.begin();
			switch (action.action) {
			case Tasks.UITask.Action.NoAction:
			case Tasks.UITask.Action.Output:
				return;
			case Tasks.UITask.Action.NewTask:
				newTask(action.newTask, false);
				return;
			case Tasks.UITask.Action.ToGame:
				newTask(null, false);
				return;
			default:
				throw new InvalidOperationException("UITask.Action enum is not sync'd with UserContext.newTask()");
			}
		}
	}

	/// <summary>
	/// Adds a single chunk of output to the buffer.
	/// </summary>
	public void outputPush(string text) {
		lock (_mutex) {
			use(true);
			_pendingOutput.Append(text);
			_pendingOutputEvent.Set();
		}
	}

	/// <summary>
	/// Removes all output from the buffer, and returns it.
	/// </summary>
	public string outputPopAll() {
		lock (_mutex) {
			use(true);
			if (_pendingOutput.Length == 0)
				return "";
			var rv = _pendingOutput.ToString();
			_pendingOutput.Clear();
			_pendingOutputEvent.Reset();
			return rv;
		}
	}

	/// <summary>
	/// Waits until some output is placed in the buffer.
	/// </summary>
	/// <returns>True if there is new output</returns>
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
			// Release the old player object, if needed.
			if (value == null && _player != null)
				_player.detach();

			_player = value;

			if (_player != null)
				_player.NewOutput = (text) => {
					outputPush(string.Format("<span>{0}</span>", text));
				};
		}
	}
	MooCore.Player _player;

	/// <summary>
	/// Get the last time the user interacted with this context.
	/// </summary>
	public DateTimeOffset lastUse {
		get { return _lastUse; }
	}

	/// <summary>
	/// Returns true if the user is in the main game state.
	/// </summary>
	public bool inGame {
		get { return _task == null; }
	}

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
	StringBuilder _pendingOutput = new StringBuilder();

	// This event is set when there is output waiting to be sent, and reset
	// when there is no output waiting.
	ManualResetEvent _pendingOutputEvent = new ManualResetEvent(false);

	// Last use time, for garbage collection.
	DateTimeOffset _lastUse = DateTimeOffset.UtcNow;

	// Current UITask for processing the user's input.
	Tasks.UITask _task;
}

}