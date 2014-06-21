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

namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// MOO Proxy object for a player object, providing all the functionality of a
/// typical mob proxy, plus some player specific functions. This is available to MOO scripts.
/// </summary>
public class PlayerProxy : MobProxy {
	public PlayerProxy(Player player) : base(player.avatar, player) {
		_player = player;
	}
	Player _player;

	/// <summary>
	/// Write the specified text to the user's terminal.
	/// </summary>
	[Passthrough]
	public void write(string text) {
		_player.write(text);
	}

	/// <summary>
	/// True if the player is actively logged in, or false if they are logged out.
	/// </summary>
	[Passthrough]
	public bool active {
		get {
			return _player.isActive;
		}
	}
}

}
