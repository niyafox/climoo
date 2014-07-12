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
/// Implements the guts of the canonical World that actually tracks all the info.
/// </summary>
public class CanonWorld : IDisposable
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
	}

	/// <summary>
	/// Pulse checks will happen every 5 seconds.
	/// </summary>
	public const int PulseFrequency = 5;

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
			Log.Info( "  Starting save callback timer" );
			_saveTimer = new Timer( (o) => saveCallback() );
			_saveTimer.Change( 30 * 1000, 30 * 1000 );

			Log.Info( "  Starting pulse timer" );
			pulseReload();
			_pulseTimer = new Timer( (o) => pulseCallback() );
			_pulseTimer.Change( PulseFrequency * 1000, PulseFrequency * 1000 );
		}
	}

	public void Dispose()
	{
		if( _saveTimer != null )
		{
			_saveTimer.Dispose();
			_saveTimer = null;
		}
		if( _pulseTimer != null )
		{
			_pulseTimer.Dispose();
			_pulseTimer = null;
		}
	}

	public long ticks
	{
		get
		{
			return _ticks;
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

	public void destroyObject( int id )
	{
		if( !_readOnly )
			_wdb.deleteMob( id );

		CanonMob junk;
		_objects.TryRemove( id, out junk );
	}

	// Because locking too much / in ways that could call back to us from Mob
	// is dangerous, what we do here is put a lock on World only to pull the items we
	// are interested in, in a minimal way. Then we let that lock go and check them all
	// for changes, holding the individual mob locks until they're done.
	void saveCallback()
	{
		// Make sure we're not already running.
		if( Interlocked.Increment( ref _saveTimerRunning ) > 1 )
		{
			Interlocked.Decrement( ref _saveTimerRunning );
			Log.Error( "Warning: stacked saveCallback() calls" );
			return;
		}

		using( new UsingAction(
			() =>
			{
				Interlocked.Decrement( ref _saveTimerRunning );
			}
		) )
		{
		/*	var toSave = new List<Mob>();
			try
			{
				// This lock is because the world objects might change.
				lock( _mutex )
				{
					foreach( var id in _objects )
					{
						MobManager mmgr = id.Value;
						Mob m = mmgr.peek;
						if( m != null )
							toSave.Add( m );
					}
				}

				foreach( var m in toSave )
				{
					// This lock is to make sure we get an atomic check on changedness and its contents.
					using( var locker = m.getLock() )
					{
						if( m.hasChanged() )
							_wdb.saveMob( m );
					}
				}
			}
			catch( Exception ex )
			{
				Log.Error( "Error during automatic save callbacks: {0}", ex );
			} */
		}
	}

	/*/// <summary>
	/// Checks to see if something should go on the pulse list (or come off it). This uses
	/// separate locking to avoid call-back deadlocking.
	/// </summary>
	public void pulseCheck( CanonMob m )
	{
		Mob mw = new Mob( m );
		int freq = mw.pulseFreq;
		if( freq == 0 )
		{
			Log.Debug( "#{0} ({1}) removed from pulse list", m.id, m.name );
			bool junk;
			_pulses.TryRemove( m, out junk );
		}
		else
		{
			Log.Debug( "#{0} ({1}) added to pulse list", m.id, m.name );
			_pulses.TryAdd( m, false );
		}
	} */

	// Called at load to dethaw any existing pulse list.
	void pulseReload()
	{
		/*var list = _wdb.mobPulseList;
		foreach( int id in list )
		{
			IMob m = findObject( id );
			if( m != null )
				pulseCheck( (CanonMob)m );
		} */
	}

	void pulseCallback()
	{
		// Make sure we're not already running.
		if( Interlocked.Increment( ref _pulseTimerRunning ) > 1 )
		{
			Interlocked.Decrement( ref _pulseTimerRunning );
			Log.Error( "Warning: stacked pulseCallback() calls" );
			return;
		}

		using( new UsingAction(
			() =>
			{
				Interlocked.Decrement( ref _pulseTimerRunning );
			}
		) )
		{
		/*	IEnumerable<Mob> toCall;
			lock( _pulseLock )
			{
				_ticks += PulseFrequency;
				toCall = _pulses.ToArray();
			}

			foreach( Mob m in toCall )
			{
				try
				{
					var freq = m.pulseFreq;
					if( freq != 0 && (_ticks % freq) == 0 )
					{
						var verb = m.pulseVerb;
						Verb v = m.verbGet( verb );
						if( v == null )
							continue;

						var param = new Verb.VerbParameters()
						{
							args = new object[] { _ticks },
							self = m,
							world = this
						};
						v.invoke( param );
					}
				}
				catch( Exception ex )
				{
					Log.Error( "Error executing pulse handler for {0}: {1}", m.id, ex );
				}
			} */
		}
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

	// Our world database instance that we use as a backing store.
	WorldDatabase _wdb;

	// Used to do periodic save checks.
	Timer _saveTimer;
	int _saveTimerRunning = 0;

	// List of objects with heartbeat verbs, plus a lock. We use a separate lock
	// here to prevent call-back deadlocks with Mob.
	ConcurrentDictionary<CanonMob, bool> _pulses = new ConcurrentDictionary<CanonMob, bool>();
	long _ticks = 0;
	Timer _pulseTimer;
	int _pulseTimerRunning = 0;

	// True if we're in "read only" mode; if so, then no database writing occurs.
	bool _readOnly;

	// >0 if we're in the middle of a merge.
	int _merging = 0;

	// This even can be waited on for a merging slot. It's advisory only: the procedure here is
	// to wait on this event and then also call for a merge slot, looping until you get it.
	ManualResetEvent _mergingEvent = new ManualResetEvent( true );
}

}
