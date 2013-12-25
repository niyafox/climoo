namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Kayateia.Climoo.Database;

public static class WorldData {
	static public void Init() {
		if (s_world == null) {
			// s_world = MooCore.World.FromSql();
			s_world = MooCore.World.FromXml(@"d:\game\export");
			if (s_world == null) {
				s_world = MooCore.World.CreateDefault();

				// This initial persist will need admin approval, to avoid
				// accidentally overwriting something else.
				s_world.saveToSql();
			}
		}
		if (s_db == null) {
			s_db = new MemoryDatabase();
			Models.XmlModelPersistence.Import(@"d:\game\export\web.xml", s_db);
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
