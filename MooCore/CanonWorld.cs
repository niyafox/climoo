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

namespace Kayateia.Climoo.MooCore
{
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ScriptHost = Scripting.SSharp.SSharpScripting;

/// <summary>
/// Implements the guts of the Vorlon^Wcanonical World that actually tracks all the info.
/// amassed in various ShadowWorlds.
/// </summary>
public class CanonWorld : IDisposable, IWorld
{
	// Only do the script init once.
	static CanonWorld()
	{
		ScriptHost.Init();

		// These are all the types we allow the scripts direct access to, including objects passed down from the outside.
		// TODO: This needs to be in a config file somewhere.
		ScriptHost.AllowType( typeof( System.Object ), "object" );
		ScriptHost.AllowType( typeof( System.String ) );
		ScriptHost.AllowType( typeof( System.Text.StringBuilder ) );
		ScriptHost.AllowType( typeof( System.Math ) );
		ScriptHost.AllowType( typeof( System.Guid ) );
		ScriptHost.AllowType( typeof( System.DateTimeOffset ), "DateTime" );
		ScriptHost.AllowType( typeof( System.TimeSpan ) );
		ScriptHost.AllowType( typeof( System.Random ) );
		ScriptHost.AllowType( typeof( System.Uri ) );
		ScriptHost.AllowType( typeof( System.UriBuilder ) );
		ScriptHost.AllowType( typeof( System.UriComponents ) );
		ScriptHost.AllowType( typeof( System.UriFormat ) );
		ScriptHost.AllowType( typeof( Builtins.Strings ) );
	}

	/// <summary>
	/// Loads a World from a database.
	/// </summary>
	/// <param name="wdb">The database</param>
	/// <param name="runtime">If true, we will set up shop for runtime usage (timers, etc)</param>
	/// <param name="readOnly">If true, no database writes will occur.</param>
	static public CanonWorld FromWorldDatabase( WorldDatabase wdb, bool runtime, bool readOnly )
	{
		return new CanonWorld( wdb, runtime, readOnly );
	}

	CanonWorld( WorldDatabase wdb, bool runtime, bool readOnly )
	{
		Log.Info( "Starting World" );

		_wdb = wdb;
		_readOnly = readOnly;

		// Load up managers for all the mobs in the world.
		int highest = -1;
		foreach( int mobId in _wdb.mobList )
		{
			_objects[mobId] = _wdb.loadMob( mobId, this );
			highest = Math.Max( highest, mobId );
		}

		// And load up the previous nextId.
		int? nid = _wdb.getConfigInt( World.ConfigNextId );
		if( !nid.HasValue )
		{
			// Just divine it from what we have. (This might be an import.)
			_nextId = highest + 1;
		}
		else
			_nextId = nid.Value;

		if( runtime )
		{
			if( !readOnly )
			{
				_saver = new SaveRunner( this, _wdb );
				_saver.start();
			}

			_pulse = new PulseRunner( World.Wrap( new ShadowWorld( this ) ) );
			_pulse.start( wdb );
		}
	}

	public void Dispose()
	{
		if( _saver != null )
		{
			_saver.Dispose();
			_saver = null;
		}

		if( _pulse != null )
		{
			_pulse.Dispose();
			_pulse = null;
		}

		// Make sure any merges are finished.
		waitMergeToken();
	}

	public long ticks
	{
		get
		{
			return _pulse.ticks;
		}
	}


	public World.UrlGenerator attributeUrlGenerator { get; set; }

	public WorldCheckpoint[] checkpoints
	{
		get
		{
			return _wdb.checkpoints.ToArray();
		}
	}

	public void checkpoint( string name )
	{
		_wdb.checkpoint( name );
	}

	public void checkpointRemove( ulong id )
	{
		if( !_readOnly )
			_wdb.checkpointRemove( id );
	}

	public CanonMob createObject()
	{
		int id = Interlocked.Increment( ref _nextId );
		if( !_readOnly )
			_wdb.setConfigInt( World.ConfigNextId, _nextId );

		CanonMob newMob = new CanonMob( this, id );
		if( !_readOnly )
			_wdb.saveMob( new Mob( newMob ) );
		_objects[id] = newMob;
		return newMob;
	}

	public CanonMob findObject( int id )
	{
		CanonMob mob;
		if( _objects.TryGetValue( id, out mob ) )
			return mob;
		else
			return null;
	}

	public IEnumerable<CanonMob> findObjects( Func<CanonMob, bool> predicate )
	{
		// This is tricky. We want to call the predicate outside of locking, but if we
		// unlock around it, the enumeration may have changed (and will fail). The only
		// reliable way to deal here is to make a copy of the list of objects up front,
		// and then unlock to do the searching.
		List<CanonMob> allmobs = new List<CanonMob>( _objects.Values );

		// Now go through the list we have and look for matches.
		var mobs = new List<CanonMob>();
		foreach( var mob in allmobs )
		{
			if( predicate( mob ) )
				mobs.Add( mob );
		}

		return mobs;
	}

	/// <summary>
	/// Callback for CanonMobs if their pulsefreq attribute is changed.
	/// </summary>
	public void pulseCheck( CanonMob m )
	{
		if( _pulse != null )
			_pulse.pulseCheck( m.id );
	}

	/// <summary>
	/// Removes everything on the delete list, returning its contents.
	/// </summary>
	/// <remarks>
	/// Should only be called during a merge slot.
	/// </remarks>
	public IEnumerable<int> emptyDeleteList()
	{
		var results = _deleted.ToArray();
		_deleted.Clear();
		return results;
	}

	/// <summary>
	/// Removes everything from the modified list, returning its contents.
	/// </summary>
	/// <remarks>
	/// Should only be called during a merge slot.
	/// </remarks>
	public IEnumerable<int> emptyModifiedList()
	{
		var results = new List<int>(
			findObjects( (cm) => cm.resetModified() ).Select( cm => cm.id )
		);
		return results;
	}

	public void destroyObject( int id )
	{
		_deleted.Add( id );

		CanonMob junk;
		_objects.TryRemove( id, out junk );
	}

	/// <summary>
	/// Token which represents permission to proceed with writing to the canon world.
	/// </summary>
	/// <remarks>
	/// Do not try to make a MergeToken of your very own. Always use the method below.
	/// </remarks>
	public class MergeToken : IDisposable
	{
		public MergeToken( CanonWorld world )
		{
			_world = world;
		}

		public void Dispose()
		{
			Interlocked.Decrement( ref _world._merging );
			_world._mergingEvent.Set();
		}

		CanonWorld _world;
	}

	/// <summary>
	/// Official interface for getting a merge slot. You need one of these before you
	/// can start writing into the canon world.
	/// </summary>
	public MergeToken getMergeToken()
	{
		int v = Interlocked.Increment( ref _merging );
		if( v == 1 )
		{
			_mergingEvent.Reset();
			return new MergeToken( this );
		}
		else
		{
			Interlocked.Decrement( ref _merging );
			return null;
		}
	}

	/// <summary>
	/// Blocks the current thread until a merge slot opens up.
	/// </summary>
	/// <param name="milliTimeout">Number of milliseconds to wait, or -1 for infinite.</param>
	public MergeToken waitMergeToken( int milliTimeout = -1 )
	{
		while( true )
		{
			MergeToken mt = getMergeToken();
			if( mt != null )
				return mt;
			if( !_mergingEvent.WaitOne( milliTimeout ) )
				return null;
		}
	}

	
	int _nextId = 1;

	// The collection of all memory-loaded Mobs. Note that this list is not necessarily
	// (and probably isn't) exhaustive of all objects in the game. They are loaded on
	// demand and occasionally dropped from the list.
	ConcurrentDictionary<int, CanonMob> _objects = new ConcurrentDictionary<int, CanonMob>();

	// List of deleted objects. This is used almost exclusively for database saving, and so
	// it will never be used outside of a merge slot. So we don't even bother to make it concurrent.
	HashSet<int> _deleted = new HashSet<int>();

	// Our world database instance that we use as a backing store.
	WorldDatabase _wdb;

	// Used to do periodic save checks.
	SaveRunner _saver;

	// True if we're in "read only" mode; if so, then no database writing occurs.
	bool _readOnly;

	// >0 if we're in the middle of a merge.
	int _merging = 0;

	// This even can be waited on for a merging slot. It's advisory only: the procedure here is
	// to wait on this event and then also call for a merge slot, looping until you get it.
	ManualResetEvent _mergingEvent = new ManualResetEvent( true );

	// Pulse timer handler, if we use it.
	PulseRunner _pulse;


	// These are pretty much here to make unit tests simpler. Not recommended for normal usage.
	#region IWorld Members

	long IWorld.ticks { get { return _pulse.ticks; } }

	World.UrlGenerator IWorld.attributeUrlGenerator { get { return null; } }

	WorldCheckpoint[] IWorld.checkpoints { get { throw new NotImplementedException(); } }
	void IWorld.checkpoint( string name ) { throw new NotImplementedException(); }
	void IWorld.checkpointRemove( ulong id ) { throw new NotImplementedException(); }

	IMob IWorld.createObject() { return createObject(); }

	IMob IWorld.findObject( int id ) { return findObject( id ); }

	IEnumerable<IMob> IWorld.findObjects( Func<IMob, bool> predicate )
	{
		return findObjects( (cm) => predicate( cm ) ).Select( m => m ).ToArray();
	}

	void IWorld.destroyObject( int id ) { destroyObject( id ); }

	#endregion
}

}
