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

	public Player(Mob avatar) {
		_avatar = avatar;
		_avatar.player = this;
	}

	public Mob avatar {
		get { return _avatar; }
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
	/// Detach this player from its in-game instance.
	/// </summary>
	public void detach() {
		this.NewOutput = null;
	}

	Mob _avatar;
}

}
