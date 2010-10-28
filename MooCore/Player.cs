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
	public event OutputNotification NewOutput;

	public Player(Mob avatar) {
		_avatar = avatar;
	}

	public Mob avatar {
		get { return _avatar; }
	}

	public void write(string text) {
		if (this.NewOutput != null) {
			string moocoded = MooCode.PrepareForClient(text);
			this.NewOutput(moocoded);
		}
	}

	Mob _avatar;
}

}
