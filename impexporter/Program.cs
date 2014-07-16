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

namespace Kayateia.Climoo.ImpExporter
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Kayateia.Climoo;
using Kayateia.Climoo.Models;
using Kayateia.Climoo.MooCore;
using Kayateia.Climoo.Database;
using Kayateia.Climoo.Database.Xml;
using Kayateia.Climoo.ImpExporter.Xml;

// This app loads up the world and web databases from MSSQL and exports them to XML files that
// can be loaded into the in-memory structures on Mono.
class Program
{
	static void Import( Info info )
	{
		Console.WriteLine( "Loading exported world database..." );

		string baseDir = info.xmlDir;
		string binDir = Path.Combine( baseDir, "bins" );

		XmlClimoo root = XmlPersistence.Load<XmlClimoo>( Path.Combine( baseDir, "mobs.xml" ) );

		// Divine the next id from the existing ones.
		int nextId = -1;
		foreach( XmlMob m in root.mobs )
			if( m.id > nextId )
				nextId = m.id;
		++nextId;

		Console.WriteLine( "Instantiating world objects..." );

		var token = info.coredb.token();

		// ?FIXME? This maybe should go through WorldDatabase too, but it gets unnecessarily
		// into the messy guts of World and Mob to do it that way.

		// Create a checkpoint. This will "overwrite" anything in the database
		// previously, but it will also allow for recovery.
		DBCheckpoint cp = new DBCheckpoint()
		{
			name = "Imported",
			time = DateTimeOffset.UtcNow
		};
		info.coredb.insert( token, cp );

		info.worlddb.setConfigInt( World.ConfigNextId, nextId );

		// Load up the mobs.
		foreach( XmlMob m in root.mobs )
		{
			DBMob dbmob = new DBMob()
			{
				objectId = m.id,
				location = m.locationId,
				owner = m.ownerId,
				parent = m.parentId,
				pathId = m.pathId,
				pulse = m.attrs.Any( a => a.name == "pulsefreq" )
			};
			info.coredb.insert( token, dbmob );

			DBMobTable dbmobtable = new DBMobTable()
			{
				mob = dbmob.id,
				objectId = m.id,
				checkpoint = cp.id
			};
			info.coredb.insert( token, dbmobtable );

			foreach( XmlAttr attr in m.attrs )
			{
				DBAttr dbattr = new DBAttr()
				{
					mime = attr.mimeType,
					name = attr.name,
					mob = dbmob.id,
					text = attr.textContents ?? null,
					data = !String.IsNullOrEmpty( attr.dataContentName )  ? File.ReadAllBytes( Path.Combine( binDir, attr.dataContentName ) ) : null
				};
				info.coredb.insert( token, dbattr );
			}

			foreach( XmlVerb verb in m.verbs )
			{
				DBVerb dbverb = new DBVerb()
				{
					name = verb.name,
					code = verb.code,
					mob = dbmob.id
				};
				info.coredb.insert( token, dbverb );
			} 
		}

		Console.WriteLine( "Importing web database..." );

		XmlClimooWeb web = XmlPersistence.Load<XmlClimooWeb>( Path.Combine( baseDir, "web.xml" ) );

		foreach (var s in web.screens) {
			info.coredb.insert( token,
				new DBScreen()
				{
					name = s.name,
					text = s.text
				}
			);
		}

		foreach (var u in web.users) {
			info.coredb.insert( token,
				new DBUser()
				{
					login = u.login,
					openId = u.openId,
					password = u.password,
					@object = u.objectId,
					name = u.name
				}
			);
		}
	}

	static void Export( Info info )
	{
		Console.WriteLine( "Loading the existing world database..." );
		CanonWorld cw = CanonWorld.FromWorldDatabase( info.worlddb, false, true );
		World w = World.Wrap( new ShadowWorld( cw ) );

		// We have a directory structure, not just an XML file, because we may also need to
		// store binary blobs like images.
		Console.WriteLine( "Saving out world database export..." );
		string baseDir = info.xmlDir;
		if( Directory.Exists( baseDir ) )
			Directory.Delete( baseDir, true );
		Directory.CreateDirectory( baseDir );

		string binDir = Path.Combine( baseDir, "bins" );
		Directory.CreateDirectory( binDir );

		var objs = w.findObjects( x => true );

		XmlClimoo root = new XmlClimoo();

		foreach( Mob m in objs )
		{
			XmlMob mob = new XmlMob()
			{
				id = m.id,
				parentId = m.parentId,
				pathId = m.pathId,
				locationId = m.locationId,
				ownerId = m.ownerId
			};
			root.mobs.Add( mob );

			foreach( var name in m.attrList )
			{
				string strval = null, binfn = null;
				var item = m.attrGet( name );
				if( item.isString )
					strval = item.str;
				else if( !item.isNull )
				{
					binfn = String.Format( "{0}-{1}.bin", m.id, name );
					File.WriteAllBytes( Path.Combine( binDir, binfn ), item.contentsAsBytes );
				}

				XmlAttr attr = new XmlAttr()
				{
					mimeType = item.mimetype,
					name = name,
					textContents = strval,
					dataContentName = binfn,
				};
				mob.attrs.Add( attr );
			}

			foreach( var name in m.verbList )
			{
				var item = m.verbGet( name );
				XmlVerb verb = new XmlVerb()
				{
					name = item.name,
					code = item.code
				};
				mob.verbs.Add( verb );
			}
		}

		XmlPersistence.Save<XmlClimoo>( Path.Combine( baseDir, "mobs.xml" ), root );

		// This holds everything not in MooCore.
		XmlClimooWeb web = new XmlClimooWeb();

		Console.WriteLine("Exporting web core database...");
		
		var token = info.coredb.token();
		web.screens.AddRange(
			from r in info.coredb.@select( token, new DBScreen(), new string[] { } )
			select new XmlScreen()
			{
				name = r.name,
				text = r.text
			});
		web.users.AddRange(
			from r in info.coredb.@select( token, new DBUser(), new string[] { } )
			select new XmlUser() {
				login = r.login,
				name = r.name,
				objectId = r.@object,
				openId = r.openId,
				password = r.password
			});

		XmlPersistence.Save<XmlClimooWeb>( Path.Combine( baseDir, "web.xml" ), web );
	}

	class Info : ImpExporterConfig
	{
		public IDatabase db;
		public CoreDatabase coredb;
		public WorldDatabase worlddb;

		public string xmlDir;
	}

    static void Main( string[] args )
	{
		if( args.Length != 3 )
		{
			Console.WriteLine( "Please specify:" );
			Console.WriteLine( " - 'import' or 'export'" );
			Console.WriteLine( " - The name of a directory where the input will come from, or where output dump will go." );
			Console.WriteLine( " - A filename of an ImpExporter database config file." );
			Console.WriteLine( "" );
			Console.WriteLine( "For example:" );
			Console.WriteLine( @"impexporter import d:\xmlfiles Kayateia.Climoo.Database.MySqlDatabase Kayateia.Climoo.DatabaseMySql d:\dbbinary" );
			return;
		}

		// Load up the config.
		ImpExporterConfig cfg = XmlPersistence.Load<ImpExporterConfig>( args[2] );

		// Try to load up the database and such first. Both directions will use it.
		Info info = new Info()
		{
			xmlDir = args[1]
		};

		Assembly asm = Assembly.Load( cfg.DatabaseAssembly );
		Type dbType = asm.GetType( cfg.DatabaseClass );
		IDatabase db = (IDatabase)Activator.CreateInstance( dbType );
		db.setup( cfg.ConnectionString, new TableInfo() );
		info.db = db;
		info.coredb = new CoreDatabase( info.db );
		info.worlddb = new WorldDatabase( info.coredb );

		if( args[0] == "import" )
			Import( info );
		else if( args[0] == "export" )
			Export( info );
		else
		{
			Console.WriteLine( "Invalid operation '{0}'", args[0] );
			return;
		}
    }
}

}
