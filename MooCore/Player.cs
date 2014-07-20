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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a user interacting with the world.
/// </summary>
public class Player {
	public delegate void OutputNotification(string text);
	public OutputNotification NewOutput;
	public OutputNotification NewSound;

	public Player( int id ) {
		_id = id;

		// Default to acting as the player.
		actorContextPush( _id );
	}

	/// <summary>
	/// This should be set whenever a context change happens (move to a new ShadowWorld).
	/// </summary>
	public World world {
		get { return _world; }
		set { _world = value; }
	}

	public int id
	{
		get { return _id; }
	}

	/// <summary>
	/// True if the user is actually logged in and active.
	/// </summary>
	public bool isActive {
		get { return this.NewOutput != null; }
	}

	/// <summary>
	/// Write the specified text to the player's console.
	/// </summary>
	public void write(string text) {
		if (this.NewOutput != null) {
			string moocoded = MooCode.PrepareForClient(text);
			this.NewOutput(moocoded);
		}
	}

	/// <summary>
	/// Plays a sound effect on the player's console.
	/// </summary>
	/// <param name="source">Mob the sound effect is located on.</param>
	/// <param name="attrName">Attribute name of the sound.</param>
	/// <param name="w">The world to use for its attribute generator.</param>
	/// <remarks>
	/// It's necessary to pass in a world here because, when this is called as part of a pulse
	/// notification, the player won't necessarily have a world.
	/// </remarks>
	public void playSound( Mob source, string attrName, World w )
	{
		if( this.NewSound != null )
		{
			string url = w.attributeUrlGenerator( source, attrName );
			this.NewSound( url );
		}
	}

	/// <summary>
	/// Detach this player from its in-game instance.
	/// </summary>
	public void detach() {
		this.NewOutput = null;
		this.NewSound = null;
	}

	/// <summary>
	/// Push an "actor context" onto the stack.
	/// </summary>
	/// <remarks>
	/// The top item on the stack will always be the acting authority for permissions
	/// checks. This stack is here so that the permissions can be inferred without
	/// having to unreliably pass around tokens.
	/// </remarks>
	/// <param name="id"></param>
	public void actorContextPush( int id )
	{
		_actors.Push( id );
	}

	/// <summary>
	/// Removes an item from the actor context stack.
	/// </summary>
	/// <remarks>See pushActorContext for more details.</remarks>
	public void actorContextPop()
	{
		_actors.Pop();
	}

	/// <summary>
	/// Returns the current actor context. This will be what code should be executing
	/// under at any given moment.
	/// </summary>
	public int actorContext
	{
		get
		{
			return _actors.Peek();
		}
	}

	Stack<int> _actors = new Stack<int>();
	World _world;
	int _id;
}

/// <summary>
/// Simple RAII class that you can wrap in a using() statement to manage the
/// actor context. This is easy to do around e.g. a verb/method call.
/// </summary>
public class ActorContext : IDisposable
{
	public ActorContext( Player player, int id )
	{
		_player = player;
		_player.actorContextPush( id );
	}

	public void Dispose()
	{
		if( _player != null )
		{
			_player.actorContextPop();
			_player = null;
		}
	}

	Player _player;
}

}
