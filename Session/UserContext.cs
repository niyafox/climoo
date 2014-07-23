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
using Kayateia.Climoo.MooCore;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserContext : IDisposable {
	public UserContext(IDatabase db) {
		_db = db;
		this.player = new Player( Mob.Anon.id );
	}

	public void Dispose() {
		Game.Login.LogUserOut(this);
	}

	/// <summary>
	/// Call when a new piece of input is received from the user.
	/// </summary>
	/// <param name="text">The new input</param>
	/// <returns>Some text to display to the user, if any.</returns>
	public string inputPush( string text, World world )
	{
		use(false);

		if( this.inGame && text == "logout" )
		{
			outputPush( new ConsoleCommand() { text = "<br/>Goodbye!<br/><br/>" } );
			Game.Login.LogUserOut(this);
			return executeCommand( "look", world );
		}
		else if( !this.inGame && text.StartsWithI( "login" ) )
		{
			string[] pieces = text.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
			if( pieces.Length != 3 )
				return "Could not log you in: incorrect syntax.";

			string result = Game.Login.LogUserIn( this, pieces[1], pieces[2] );
			if( result != null )
				return "Could not log you in: " + result;

			outputPush( "\nSuddenly you're falling...!\n\n" );
			return executeCommand( "look", world );
		}
		else
		{
			return executeCommand( text, world );
		}
	}

	// Executes a MOO command.
	string executeCommand( string text, World world )
	{
		try {
			this.player.world = world;
			return MooCore.InputParser.ProcessInput(text, this.player);
		} catch (System.Exception ex) {
			return "<span class=\"error\">Exception: {0}".FormatI(ex.ToString());
		}
		finally
		{
			world.waitForMerge();
			this.player.world = null;
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
					outputPush( text );
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
		get { return _player.anonMob == null; }
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

	// Database instance we'll be using to access various things.
	IDatabase _db;
}

}
