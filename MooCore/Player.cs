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

	World _world;
	int _id;
}

}
