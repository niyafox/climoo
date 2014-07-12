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

namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Kayateia.Climoo.Database;
using Kayateia.Climoo.MooCore;

// This is more of an integration test, really, but this stuff needs some testing.
[TestFixture]
public class WorldTest
{
	[Test]
	public void Basic()
	{
		createBasicWorld();

		StringBuilder sb = new StringBuilder();
		_w.findObjects( (mob) =>
		{
			sb.AppendLine( printObject( mob ) );
			return false;
		} );

		TestCommon.CompareRef( System.IO.Path.Combine( "WorldTest", "Basic" ), sb.ToString() );
	}

	string printObject( Mob m )
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine( CultureFree.Format( "{0} (#{1}) - at #{2}, parent #{3}, path {4}", m.name, m.id, m.locationId, m.parentId, m.fqpn ) );
		sb.AppendLine( m.desc );
		foreach( var vn in m.verbList )
		{
			Verb v = m.verbGet( vn );
			sb.AppendLine( CultureFree.Format( "Verb {0}: {1}", v.name, v.code.Replace( "\n", "-cr-" ) ) );
		}
		foreach( var an in m.attrList )
		{
			TypedAttribute a = m.attrGet( an );
			sb.AppendLine( CultureFree.Format( "Attr {0}: {1}", an, a.str ) );
		}
		sb.AppendLine();

		return sb.ToString();
	}

	void createBasicWorld()
	{
		createBasicDatabase();
		_cw = CanonWorld.FromWorldDatabase( _wdb, false, false );
		_sw = new ShadowWorld( _cw );
		_w = new World( _sw );
	}

	// This makes a basic world with 5 objects:
	// God (#1) /
	// Templates (#2) /templates
	// PlayerTemplate (#3) /templates/player
	// Player (#4)
	// Test (#5)
	void createBasicDatabase()
	{
		var mdb = new MemoryDatabase();
		var cdb = new CoreDatabase( mdb );
		_wdb = new WorldDatabase( cdb );

		_wdb.setConfigInt( World.ConfigNextId, 6 );

		// Create a new checkpoint.
		using( var token = cdb.token() )
		{
			DBCheckpoint cp = new DBCheckpoint()
			{
				name = "initial",
				time = DateTimeOffset.UtcNow
			};
			cdb.insert( token, cp );
		}

		// This will let our stubs function in a minimal way for saving.
		_stubworld = new StubWorld();

		// Create a god object.
		_god = makeMob( 1, 0, 0, "God", "The god object!" );

		// Create a template room.
		_templates = makeMob( 2, 1, 1, "Templates", "Template room", (m) => { m.pathId = "templates"; } );

		// Create a player template.
		_playerTemplate = makeMob( 3, 1, 2, "PlayerTemplate", "Playter template", (m) => { m.pathId = "player"; } );

		// Create a player.
		_player = makeMob( 4, 3, 1, "Player", "Test player" );

		_testObj = makeMob( 5, 1, 1, "Test", "Test object" );
	}

	Mob makeMob( int id, int parentId, int locationId, string name, string desc, Action<Mob> postprep = null )
	{
		Mob mob = Mob.Wrap( new StubMob( id:id, parentId:parentId )
		{
			locationId = locationId,
			perms = 0,
			world = _stubworld
		} );
		mob.attrSet( Mob.Attributes.Name, name );
		mob.attrSet( Mob.Attributes.Description, desc );
		if( postprep != null )
			postprep( mob );
		_stubworld.addObject( (StubMob)mob.get );
		_wdb.saveMob( mob );

		return mob;
	}

	Mob _god, _templates, _playerTemplate, _player, _testObj;
	WorldDatabase _wdb;
	CanonWorld _cw;
	ShadowWorld _sw;
	StubWorld _stubworld;
	World _w;
}

}
