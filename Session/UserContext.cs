/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Kayateia.Climoo.Session {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Text;

using Kayateia.Climoo.Database;
using Kayateia.Climoo.Models;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserContext : IDisposable {
	public UserContext(IDatabase db) {
		_db = db;

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
				outputPush( new ConsoleCommand() { text = "<br/>Goodbye!<br/><br/>" } );
				Game.Login.LogUserOut(this);
				newTask(new Tasks.PublicSite(this));
				return "";
			} else {
				try {
					return MooCore.InputParser.ProcessInput(text, this.player);
				} catch (System.Exception ex) {
					return "<span class=\"error\">Exception: {0}".FormatI(ex.ToString());
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
	public void outputPush( ConsoleCommand cmd )
	{
		lock( _mutex )
		{
			use( true );
			_pendingOutput.Enqueue( cmd );
			_pendingOutputEvent.Set();
		}
	}

	/// <summary>
	/// Convenience method that adds a chunk of output to the buffer.
	/// </summary>
	public void outputPush( string text )
	{
		outputPush( new ConsoleCommand() { text = text } );
	}

	/// <summary>
	/// Removes all output from the buffer, and returns it.
	/// </summary>
	public ConsoleCommand outputPop()
	{
		lock( _mutex )
		{
			use( true );

			var rv = _pendingOutput.Dequeue();
			if( _pendingOutput.Count == 0 )
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

			if( _player != null )
			{
				_player.NewOutput = (text) =>
				{
					outputPush( string.Format( "<div class=\"output-block\">{0}</div>", text ) );
				};
				_player.NewSound = (url) =>
				{
					outputPush( new ConsoleCommand() { sound = url } );
				};
			}
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

	/// <summary>
	/// The database interface to use for accessing various things.
	/// </summary>
	public IDatabase db {
		get { return _db; }
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
	Queue<ConsoleCommand> _pendingOutput = new Queue<ConsoleCommand>();

	// This event is set when there is output waiting to be sent, and reset
	// when there is no output waiting.
	ManualResetEvent _pendingOutputEvent = new ManualResetEvent(false);

	// Last use time, for garbage collection.
	DateTimeOffset _lastUse = DateTimeOffset.UtcNow;

	// Current UITask for processing the user's input.
	Tasks.UITask _task;

	// Database instance we'll be using to access various things.
	IDatabase _db;
}

}
