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
	using System.Reflection;

/// <summary>
/// Container for the active world data.
/// </summary>
public static class WorldData {
	static public void Init() {
		if( s_world != null && s_db != null)
			return;

		// Load up all the strings of interest from the web.config file.
		string dbString = System.Configuration.ConfigurationManager.ConnectionStrings["climoo_dbcConnectionString"].ConnectionString;
		string dbClass = System.Configuration.ConfigurationManager.ConnectionStrings["climoo_dbcConnectionString"].ProviderName;
		string dbFsString = System.Configuration.ConfigurationManager.ConnectionStrings["climoo_dbcFileSystemString"].ConnectionString;
		string xmlString = System.Configuration.ConfigurationManager.ConnectionStrings["climoo_xmlImportPathString"].ConnectionString;

		// We'll use this in multiple places below.
		var ti = new TableInfo();

		// Break up the pieces of the connection class to figure out what to load.
		string[] classPieces = dbClass.Split( ',' ).Select( x => x.Trim() ).ToArray();
		if( classPieces.Length < 2 )
			throw new ArgumentException( "Too few comma-delimited strings in the database connection class string", dbClass );

		string className = classPieces[0];
		string asmName = classPieces[1];

		// This assembly we want is possibly already loaded, but this gets us a handle to it.
		Assembly dbAsm = Assembly.Load( asmName );
		Type dbType = dbAsm.GetType( className );

		// Create the actual database class.
		IDatabase db = (IDatabase)(Activator.CreateInstance( dbType ));
		db.connect( dbString, dbFsString, ti );

		if (s_world == null) {
			// s_world = MooCore.World.FromXml( xmlString );
			// var db = new MySqlDatabase();
			var coredb = new CoreDatabase( db );
			var wdb = new MooCore.WorldDatabase( coredb );
			s_world = MooCore.World.FromWorldDatabase( wdb );
		}
		if (s_db == null) {
			s_db = db;
			// s_db = new MemoryDatabase();
			// Models.XmlModelPersistence.Import(System.IO.Path.Combine(basePath, "web.xml"), s_db);
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
