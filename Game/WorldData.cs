namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class WorldData {
	static public void Init() {
		if (s_world == null) {
			s_world = MooCore.World.FromSql();
			if (s_world == null) {
				s_world = MooCore.World.CreateDefault();

				// This initial persist will need admin approval, to avoid
				// accidentally overwriting something else.
				s_world.saveToSql();
			}
		}
	}

	static public MooCore.World world {
		get {
			Init();
			return s_world;
		}
	}

	static MooCore.World s_world;
}

}
