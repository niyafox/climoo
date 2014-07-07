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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ScriptHost = Scripting.SSharp.SSharpScripting;

/// <summary>
/// Constants for special (well-known) object paths.
/// </summary>
public class SpecialObjects
{
	public const string Root = "/";
	public const string Player = "/templates/player";
	public const string Room = "/templates/room";
	public const string Portal = "/templates/portal";

	// Attributes of relevance: wait, interval, timerverb.
	public const string Timer = "/templates/timer";
}

/// <summary>
/// The world: a managed collection of objects.
/// </summary>
/// <remarks>
/// This object should be disposed if used in runtime mode.
/// </remarks>
public partial class World : IDisposable {
	// Only do the script init once.
	static World() {
		ScriptHost.Init();

		// These are all the types we allow the scripts direct access to, including objects passed down from the outside.
		// TODO: This needs to be in a config file somewhere.
		ScriptHost.AllowType(typeof(System.Object), "object");
		ScriptHost.AllowType(typeof(System.String));
		ScriptHost.AllowType(typeof(System.Text.StringBuilder));
		ScriptHost.AllowType(typeof(System.Math));
		ScriptHost.AllowType(typeof(System.Guid));
		ScriptHost.AllowType(typeof(System.DateTimeOffset), "DateTime");
		ScriptHost.AllowType(typeof(System.TimeSpan));
		ScriptHost.AllowType(typeof(System.Random));
		ScriptHost.AllowType(typeof(System.Uri));
		ScriptHost.AllowType(typeof(System.UriBuilder));
		ScriptHost.AllowType(typeof(System.UriComponents));
		ScriptHost.AllowType(typeof(System.UriFormat));
	}

	World() {
	}

	/// <summary>
	/// Loads a World from a database.
	/// </summary>
	/// <param name="wdb">The database</param>
	/// <param name="runtime">If true, we will set up shop for runtime usage (timers, etc)</param>
	static public World FromWorldDatabase( WorldDatabase wdb, bool runtime )
	{
		return new World( wdb, runtime );
	}

	public const string ConfigNextId = "nextid";

	World( WorldDatabase wdb, bool runtime )
	{
		Log.Info( "Starting World" );

		_wdb = wdb;

		// Load up managers for all the mobs in the world.
		int highest = -1;
		foreach( int mobId in _wdb.mobList )
		{
			MobManager mgr = new MobManager( mobId, this, _wdb, null );
			_objects[mobId] = mgr;
			highest = Math.Max( highest, mobId );
		}

		// And load up the previous nextId.
		int? nid = _wdb.getConfigInt( ConfigNextId );
		if( !nid.HasValue )
		{
			// Just divine it from what we have. (This might be an import.)
			_nextId = highest + 1;
		}
		else
			_nextId = nid.Value;

		if( runtime )
		{
			Log.Info( "  Starting save callback timer" );
			_saveTimer = new Timer( (o) => saveCallback() );
			_saveTimer.Change( 30 * 1000, 30 * 1000 );
		}
	}

	public void Dispose()
	{
		if( _saveTimer != null )
		{
			_saveTimer.Dispose();
			_saveTimer = null;
		}
	}

	public delegate string UrlGenerator(Mob obj, string name);
	
	/// <summary>
	/// This delegate must return valid URLs for attribute data. This lets us produce web
	/// links for things like images.
	/// </summary>
	public UrlGenerator attributeUrlGenerator = null;

	/// <summary>
	/// Gets the list of checkpoints from the world database and passes on the savings.
	/// </summary>
	public WorldCheckpoint[] checkpoints
	{
		get
		{
			return _wdb.checkpoints.ToArray();
		}
	}

	/// <summary>
	/// Performs a checkpoint in the world database.
	/// </summary>
	public void checkpoint( string name )
	{
		_wdb.checkpoint( name );
	}

	/// <summary>
	/// Performs a checkpoint in the world database.
	/// </summary>
	public void checkpointRemove( int id )
	{
		_wdb.checkpointRemove( id );
	}

	/// <summary>
	/// Basic logic for creating objects in the world database.
	/// </summary>
	/// <remarks>
	/// This is very raw and any mob created here must have quite a bit of work done to it before it's final.
	/// </remarks>
	public Mob createObject()
	{
		lock( _mutex )
		{
			int id = _nextId++;
			_wdb.setConfigInt( ConfigNextId, _nextId );

			Mob newMob = new Mob( this, id );
			_wdb.saveMob( newMob );
			_objects[id] = new MobManager( id, this, _wdb, newMob );
			return newMob;
		}
	}

	/// <summary>
	/// Convenience method for creating objects inline with attributes.
	/// </summary>
	/// <returns>
	/// The new mob.
	/// </returns>
	/// <param name='attributes'>An object with properties describing attributes for the new mob</param>
	/// <param name='location'>The new mob's location</param>
	/// <param name='parent'>The new mob's OOP parent</param>
	public Mob createObject( object attributes, int? location = null, int? parent = null )
	{
		Mob newMob = createObject();
		foreach( var item in PropertyEnumerator.GetProperties( attributes ) )
			newMob.attrSet( item.Name, item.Value );
		if( location.HasValue )
			newMob.locationId = location.Value;

		// Objects are parented onto the PTB by default. Otherwise...
		if( parent.HasValue )
			newMob.parentId = parent.Value;

		// Save the more completed version too.
		_wdb.saveMob( newMob );

		return newMob;
	}

	/// <summary>
	/// Locates an existing mob by ID.
	/// </summary>
	public Mob findObject( int id )
	{
		lock( _mutex )
		{
			if( _objects.ContainsKey(id) )
			{
				return _objects[id].get;
			}
			else
				return null;
		}
	}

	/// <summary>
	/// Locates an existing mob by fully qualified path name.
	/// </summary>
	public Mob findObject( string path )
	{
		if( string.IsNullOrEmpty(path) )
			return null;

		string[] components = path.Split( Mob.PathSep );
		Mob cur;
		if( components[0].StartsWith("#") )
			cur = findObject( CultureFree.ParseInt( components[0].Substring(1) ) );
		else
			cur = findObject(1);	// ptb

		for( int i=1; i<components.Length; ++i )
		{
			if( components[i].StartsWithI("#") )
				throw new ArgumentException( "Path contains more than one absolute component" );
			cur = findObject( (m) =>
				cur.id == m.get.locationId &&
				components[i] == m.get.pathId
			);
			if( cur == null )
				return null;
		}

		return cur;
	}

	/// <summary>
	/// Locates an object by search predicate.
	/// </summary>
	/// <remarks>
	/// We pass out MobManagers here so that the predicate function can decide whether
	/// or not to crack the shell if it wants to search inside.
	/// </remarks>
	public Mob findObject( Func<MobManager, bool> predicate )
	{
		return findObjects( predicate ).FirstOrDefault();
	}

	/// <summary>
	/// Locates many objects by search predicate.
	/// </summary>
	public IEnumerable<Mob> findObjects( Func<MobManager, bool> predicate ) {
		lock( _mutex )
		{
			// We build an array here to avoid lock slicing.
			// Amusing note: This method was considerably more attractive (yield return and such)
			// until it broke the compiler in VS2010. So I had to dumb it down.
			var mobs = new List<Mob>();
			foreach( var mob in _objects )
			{
				if( predicate( mob.Value ) )
					mobs.Add( mob.Value.get );
			}

			return mobs.ToArray();
		}
	}

	/// <summary>
	/// Destroys an object.
	/// </summary>
	public void destroyObject( int id )
	{
		lock( _mutex )
		{
			_wdb.deleteMob( id );
			_objects.Remove( id );
		}
	}

	void saveCallback()
	{
		lock( _mutex )
		{
			try
			{
				foreach( var id in _objects )
				{
					MobManager mmgr = id.Value;
					Mob m = mmgr.peek;
					if( m == null )
						continue;

					if( m.hasChanged() )
					{
						_wdb.saveMob( m );
						m.resetChanged();
					}
				}
			}
			catch( Exception ex )
			{
				Log.Error( "Error during automatic save callbacks: {0}", ex );
			}
		}
	}

	object _mutex = new object();
	int _nextId = 1;

	// The collection of all memory-loaded Mobs. Note that this list is not necessarily
	// (and probably isn't) exhaustive of all objects in the game. They are loaded on
	// demand and occasionally dropped from the list.
	Dictionary<int, MobManager> _objects = new Dictionary<int, MobManager>();

	// Our world database instance that we use as a backing store.
	WorldDatabase _wdb;

	// Used to do periodic save checks.
	Timer _saveTimer;
}

}
