/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Kayateia.Climoo.Database;

/// <summary>
/// Container for the active world data.
/// </summary>
public static class WorldData {
	static public void Init() {
		string projectPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
		string basePath = System.IO.Path.Combine(projectPath, "notes", "data-export");
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
