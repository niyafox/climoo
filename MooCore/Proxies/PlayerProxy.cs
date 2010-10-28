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

	public void write(string text) {
		_player.write(text);
	}

	public override bool isMethodPassthrough(string name) {
		return name == "write" || base.isMemberPassthrough(name);
	}
}

}
