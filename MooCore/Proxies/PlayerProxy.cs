namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class PlayerProxy : MobProxy {
	public PlayerProxy(Player player) : base(player.avatar, player) {
		_player = player;
	}
	Player _player;

	[Passthrough]
	public void write(string text) {
		_player.write(text);
	}

	[Passthrough]
	public bool active {
		get {
			return _player.isActive;
		}
	}
}

}
