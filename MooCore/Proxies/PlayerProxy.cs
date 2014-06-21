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
