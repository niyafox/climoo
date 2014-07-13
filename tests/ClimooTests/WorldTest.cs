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
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Kayateia.Climoo.Database;
using Kayateia.Climoo.MooCore;

// This is more of an integration test, really, but this stuff needs some testing.
[TestFixture]
public class WorldTest
{
	// A really basic test that just creates the test world and prints it out.
	[Test]
	public void Basic()
	{
		createBasicWorld();

		string results = printWorld( _w );
		TestCommon.CompareRef( Path.Combine( "WorldTest", "Basic" ), results );
	}

	// Tests basic merging: make a change, print out the world.
	[Test]
	public void Merge()
	{
		createBasicWorld();

		StringBuilder sb = new StringBuilder();

		_testObj.attrSet( "test", "This is a test attribute." );
		_testObj.verbSet( "test", new Verb() { name = "test", code = "// This is a test verb." } );

		sb.AppendLine( "Easy merge version:" );
		sb.AppendLine( "Shadow world:" );
		sb.AppendLine( printWorld( _w ) );

		sb.AppendLine( "Canon world:" );
		sb.AppendLine( printCanon() );

		sb.AppendLine();

		sb.AppendLine( "Harder merge version:" );
		using( var token = _cw.getMergeToken() )
		{
			_testObj.attrSet( "test2", "This is a test attribute 2." );
			_testObj.verbSet( "test2", new Verb() { name = "test2", code = "// This is a test verb 2." } );

			sb.AppendLine( "Shadow world:" );
			sb.AppendLine( printWorld( _w ) );

			sb.AppendLine( "Canon world:" );
			sb.AppendLine( printCanon() );
		}
		_sw.waitForMerge();

		sb.AppendLine( "Shadow world:" );
		sb.AppendLine( printWorld( _w ) );

		sb.AppendLine( "Canon world:" );
		sb.AppendLine( printCanon() );

		string results = sb.ToString();
		TestCommon.CompareRef( Path.Combine( "WorldTest", "Merge" ), results );
	}

	// Tests merges under conflict.
	[Test]
	public void ConflictMerge()
	{
		createBasicWorld();

		StringBuilder sb = new StringBuilder();

		// Make a second shadow world to work with, to simluate two threads.
		ShadowWorld sw2 = new ShadowWorld( _cw );
		World w2 = new World( sw2 );
		Mob _test2 = Mob.Wrap( sw2.findObject( _testObj.id ) );

		// Disable immediate updates.
		using( var token = _cw.getMergeToken() )
		{
			// Do some interleaved writes.
			_testObj.attrSet( "testA", "Value A1" );
			_test2.attrSet( "testA", "Value A2" );
			_test2.attrSet( "testB", "Value B2" );
			_testObj.attrSet( "testB", "Value B1" );

			_testObj.verbSet( "testA", new Verb() { name = "testA", code = "//Value A1" } );
			_test2.verbSet( "testA", new Verb() { name = "testA", code = "//Value A2" } );
			_test2.verbSet( "testB", new Verb() { name = "testB", code = "//Value B2" } );
			_testObj.verbSet( "testB", new Verb() { name = "testB", code = "//Value B1" } );
		}
		_sw.waitForMerge();
		sw2.waitForMerge();

		sb.AppendLine( "Simultaneous merge:" );
		sb.AppendLine( "Canon:" );
		sb.AppendLine( printCanon() );
		sb.AppendLine( "Shadow A:" );
		sb.AppendLine( printWorld( _w ) );
		sb.AppendLine( "Shadow B:" );
		sb.AppendLine( printWorld( w2 ) );

		// The second one will test one object getting a chance to update, and a second coming in later.
		_testObj.attrSet( "testC", "Value C1" );
		using( var token = _cw.getMergeToken() )
		{
			_test2.attrSet( "testC", "Value C2" );
			_test2.attrSet( "testD", "Value D2" );
		}
		_testObj.attrSet( "testD", "Value D1" );

		using( var token = _cw.getMergeToken() )
		{
			_testObj.verbSet( "testC", new Verb() { name = "testC", code = "//Value C1" } );
			_test2.verbSet( "testC", new Verb() { name = "testC", code = "//Value C2" } );
		}
		_test2.verbSet( "testD", new Verb() { name = "testD", code = "//Value D2" } );
		_testObj.verbSet( "testD", new Verb() { name = "testD", code = "//Value D1" } );

		_sw.waitForMerge();
		sw2.waitForMerge();

		sb.AppendLine( "Interleaved merge:" );
		sb.AppendLine( "Canon:" );
		sb.AppendLine( printCanon() );
		sb.AppendLine( "Shadow A:" );
		sb.AppendLine( printWorld( _w ) );
		sb.AppendLine( "Shadow B:" );
		sb.AppendLine( printWorld( w2 ) );

		string results = sb.ToString();
		TestCommon.CompareRef( Path.Combine( "WorldTest", "ConflictMerge" ), results );
	}

	[Test]
	public void DeleteMerge()
	{
		createBasicWorld();

		StringBuilder sb = new StringBuilder();

		// Make a second shadow world to work with, to simluate two threads.
		ShadowWorld sw2 = new ShadowWorld( _cw );
		World w2 = new World( sw2 );
		Mob _test2 = Mob.Wrap( sw2.findObject( _testObj.id ) );

		// Insert some values and make sure they show up for everyone.
		_testObj.attrSet( "test1", "Test 1" );
		_testObj.attrSet( "test2", "Test 1" );

		sb.AppendLine( "Pre-test:" );
		sb.AppendLine( "Canon:" );
		sb.AppendLine( printCanon() );
		sb.AppendLine( "Shadow A:" );
		sb.AppendLine( printWorld( _w ) );
		sb.AppendLine( "Shadow B:" );
		sb.AppendLine( printWorld( w2 ) );

		// Now make some delete changes under conflict.
		using( var token = _cw.getMergeToken() )
		{
			_testObj.attrSet( "test1", "Test 2" );
			_test2.attrDel( "test1" );

			_test2.attrDel( "test2" );
			_testObj.attrSet( "test2", "Test 2" );
		}

		_sw.waitForMerge();
		sw2.waitForMerge();

		sb.AppendLine( "Post-test:" );
		sb.AppendLine( "Canon:" );
		sb.AppendLine( printCanon() );
		sb.AppendLine( "Shadow A:" );
		sb.AppendLine( printWorld( _w ) );
		sb.AppendLine( "Shadow B:" );
		sb.AppendLine( printWorld( w2 ) );

		string results = sb.ToString();
		TestCommon.CompareRef( Path.Combine( "WorldTest", "DeleteMerge" ), results );
	}

	string printCanon()
	{
		StringBuilder sb = new StringBuilder();
		_cw.findObjects( (mob) =>
		{
			sb.AppendLine( printObject( Mob.Wrap( mob ) ) );
			return false;
		} );

		return sb.ToString();
	}

	string printWorld( World w )
	{
		StringBuilder sb = new StringBuilder();
		_w.findObjects( (mob) =>
		{
			sb.AppendLine( printObject( mob ) );
			return false;
		} );

		return sb.ToString();
	}

	string printObject( Mob m )
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine( CultureFree.Format( "{0} (#{1}) - at #{2}, parent #{3}, path {4}", m.name, m.id, m.locationId, m.parentId, m.fqpn ) );
		sb.AppendLine( m.desc );

		// The "deleted" should only ever happen while printing a canon world.
		foreach( var vn in m.verbList )
		{
			Verb v = m.verbGet( vn );
			if( v != null )
				sb.AppendLine( CultureFree.Format( "Verb {0}: {1}", v.name, v.code.Replace( "\n", "-cr-" ) ) );
			else
				sb.AppendLine( CultureFree.Format( "Verb {0} [deleted]", v.name ) );
		}
		foreach( var an in m.attrList )
		{
			TypedAttribute a = m.attrGet( an );
			if( a != null )
				sb.AppendLine( CultureFree.Format( "Attr {0}: {1}", an, a.str ) );
			else
				sb.AppendLine( CultureFree.Format( "Attr {0} [deleted]", an ) );
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

		// We want to replace these with actual world copies now.
		_god = _w.findObject( 1 );
		_templates = _w.findObject( 2 );
		_playerTemplate = _w.findObject( 3 );
		_player = _w.findObject( 4 );
		_testObj = _w.findObject( 5 );
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
