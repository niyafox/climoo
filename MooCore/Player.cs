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

	public bool isActive {
		get { return this.NewOutput != null; }
	}

	public void write(string text) {
		if (this.NewOutput != null) {
			string moocoded = MooCode.PrepareForClient(text);
			this.NewOutput(moocoded);
		}
	}

	public void detach() {
		this.NewOutput = null;
	}

	Mob _avatar;
}

}
