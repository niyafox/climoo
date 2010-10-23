namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class WorldData {
	static public void Init() {
		if (s_world == null)
			s_world = MooCore.World.CreateDefault();
	}

	static public MooCore.World world {
		get {
			if (s_world == null)
				s_world = MooCore.World.CreateDefault();
			return s_world;
		}
	}

	static MooCore.World s_world;
}

}
