namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class WorldData {
	static public void Init() {
		if (s_world == null)
			s_world = MooEngine.World.CreateDefault();
	}

	static public MooEngine.World world {
		get {
			if (s_world == null)
				s_world = MooEngine.World.CreateDefault();
			return s_world;
		}
	}

	static MooEngine.World s_world;
}

}
