namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Kayateia.Climoo.Database;

public static class WorldData {
	static public void Init() {
		string basePath;
		if (System.IO.Path.DirectorySeparatorChar == '/') {
			basePath = "/Users/kaya/Windows/climoo/export";
		} else {
			basePath = @"d:\game\export";
		}
		if (s_world == null) {
			// s_world = MooCore.World.FromSql();
			s_world = MooCore.World.FromXml(basePath);
			if (s_world == null) {
				s_world = MooCore.World.CreateDefault();

				// This initial persist will need admin approval, to avoid
				// accidentally overwriting something else.
				s_world.saveToSql();
			}
		}
		if (s_db == null) {
			s_db = new MemoryDatabase();
			Models.XmlModelPersistence.Import(System.IO.Path.Combine(basePath, "web.xml"), s_db);
		}
	}

	static public MooCore.World world {
		get {
			Init();
			return s_world;
		}
	}

	static public IDatabase db {
		get {
			Init();
			return s_db;
		}
	}

	static MooCore.World s_world;
	static IDatabase s_db;
}

}
