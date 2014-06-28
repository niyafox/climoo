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

using System;
using System.Collections.Generic;

namespace Kayateia.Climoo.Database {

[Table( Name = "mob" )]
public class DBMob : TableRow<DBMob>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column( Name = "objectid" )]
	public int objectId { get; set; }

	[Column( Name = "parentid", Nullable = true )]
	public int? parent { get; set; }

	[Column( Name = "pathid" )]
	public string pathId { get; set; }

	[Column( Name = "locationid", Nullable = true )]
	public int? location { get; set; }

	[Column]
	public int perms { get; set; }

	[Column( Name = "ownerid" )]
	public int owner { get; set; }
}

[Table( Name = "verb" )]
public class DBVerb : TableRow<DBVerb>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column]
	public string name { get; set; }

	[Column( Big = true )]
	public string code { get; set; }

	[Column( Name = "mobid" )]
	public int mob { get; set; }

	[Column]
	public int perms { get; set; }
}

[Table( Name = "attribute" )]
public class DBAttr : TableRow<DBAttr>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column( Name="textcontents", Big = true, Nullable = true )]
	public string text { get; set; }

	[Column( Name="datacontents", Big = true, Nullable = true, Binary = true )]
	public byte[] data { get; set; }

	[Column]
	public string name { get; set; }

	[Column( Name = "mimetype" )]
	public string mime { get; set; }

	[Column( Name = "mobid" )]
	public int mob { get; set; }

	[Column]
	public int perms { get; set; }
}

[Table( Name = "mobtable" )]
public class DBMobTable : TableRow<DBMobTable>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column( Name = "mobid" )]
	public int mob { get; set; }

	// This is technically a denormalization, but it makes selecting a lot simpler.
	[Column( Name = "objectid" )]
	public int objectId { get; set; }

	[Column( Name = "checkpointid" )]
	public int checkpoint { get; set; }
}

[Table( Name = "checkpoint" )]
public class DBCheckpoint : TableRow<DBCheckpoint>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column]
	public DateTimeOffset time { get; set; }

	[Column]
	public string name { get; set; }
}

[Table( Name = "config" )]
public class DBConfig : TableRow<DBConfig>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column]
	public string name { get; set; }

	[Column( Nullable = true )]
	public int? intvalue { get; set; }

	[Column( Nullable = true )]
	public string strvalue { get; set; }

	[Column( Name = "checkpointid" )]
	public int checkpoint { get; set; }
}

[Table( Name = "screen" )]
public class DBScreen : TableRow<DBScreen>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column]
	public string name { get; set; }

	[Column( Big = true )]
	public string text { get; set; }
}

[Table( Name = "user" )]
public class DBUser : TableRow<DBUser>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column]
	public string login { get; set; }

	[Column( Name = "openid" )]
	public bool openId { get; set; }

	[Column]
	public string password { get; set; }

	[Column( Name = "objectid" )]
	public int @object { get; set; }

	[Column]
	public string name { get; set; }
}

[Table( Name = "test" )]
public class DBTest : TableRow<DBTest>
{
	[Column( PK = true )]
	public int id { get; set; }

	[Column( Nullable = true )]
	public string str { get; set; }

	[Column( Big = true, Nullable = true )]
	public string longer { get; set; }

	[Column( Nullable = true )]
	public int num { get; set; }

	[Column( Big = true, Binary = true, Nullable = true )]
	public byte[] datacol { get; set; }

	[Column( Name = "bool", Nullable = true )]
	public bool boolean { get; set; }

	[Column( Nullable = true )]
	public DateTimeOffset time { get; set; }
}

public class TableInfo : ITableInfo
{
	static Dictionary<string, Type> s_tables = new Dictionary<string, Type>()
	{
		{ "mob", typeof( DBMob ) },
		{ "verb", typeof( DBVerb ) },
		{ "attribute", typeof( DBAttr ) },
		{ "mobtable", typeof( DBMobTable ) },
		{ "checkpoint", typeof( DBCheckpoint ) },
		{ "config", typeof( DBConfig ) },
		{ "screen", typeof( DBScreen ) },
		{ "user", typeof( DBUser ) },
		{ "test", typeof( DBTest ) }
	};
	public string getIdColumn( string table )
	{
		if( !s_tables.ContainsKey( table ) )
			throw new ArgumentException( "No such table", table );

		Type t = s_tables[table];
		return TableRow.GetPKName( t );
	}

	public bool isBinary( string table, string dbName )
	{
		if( !s_tables.ContainsKey( table ) )
			throw new ArgumentException( "No such table", table );

		Type t = s_tables[table];
		string objName = TableRow.GetColumnObjName( t, dbName );
		return TableRow.GetColumnAttr( t, objName ).Binary;
	}

	public Type getColumnType( string table, string dbName )
	{
		if( !s_tables.ContainsKey( table ) )
			throw new ArgumentException( "No such table", table );

		Type t = s_tables[table];
		string objName = TableRow.GetColumnObjName( t, dbName );
		return TableRow.GetColumnType( t, objName );
	}

	public IEnumerable<string> getAllColumns( string table )
	{
		if( !s_tables.ContainsKey( table ) )
			throw new ArgumentException( "No such table", table );

		Type t = s_tables[table];
		return TableRow.GetDBColumns( t );
	}
}

}
