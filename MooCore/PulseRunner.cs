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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// Handles the world pulse callbacks.
/// </summary>
/// <remarks>
/// This is separated out from the CanonWorld itself, because in reality, it is
/// just another ShadowWorld consumer like anything else.
/// </remarks>
public class PulseRunner : IDisposable
{
	public PulseRunner( World w )
	{
		_world = w;
	}

	public void start( WorldDatabase wdb )
	{
		Log.Info( "  Starting pulse timer" );
		pulseReload( wdb );
		_pulseTimer = new Timer( (o) => pulseCallback() );
		_pulseTimer.Change( PulseFrequency * 1000, PulseFrequency * 1000 );
	}

	public void Dispose()
	{
		if( _pulseTimer != null )
		{
			_pulseTimer.Dispose();
			_pulseTimer = null;
		}
	}

	/// <summary>
	/// Pulse checks will happen every 5 seconds.
	/// </summary>
	public const int PulseFrequency = 5;

	public long ticks
	{
		get
		{
			return _ticks;
		}
	}

	/// <summary>
	/// Checks to see if something should go on the pulse list (or come off it). This uses
	/// separate locking to avoid call-back deadlocking.
	/// </summary>
	public void pulseCheck( int id )
	{
		Mob m = _world.findObject( id );
		int freq = m.pulseFreq;
		if( freq == 0 )
		{
			Log.Debug( "#{0} ({1}) removed from pulse list", m.id, m.name );
			bool junk;
			_pulses.TryRemove( m.id, out junk );
		}
		else
		{
			Log.Debug( "#{0} ({1}) added to pulse list", m.id, m.name );
			_pulses.TryAdd( m.id, false );
		}
	}

	public void deleted( int id )
	{
		bool junk;
		_pulses.TryRemove( id, out junk );
	}

	// Called at load to dethaw any existing pulse list.
	void pulseReload( WorldDatabase wdb )
	{
		var list = wdb.mobPulseList;
		foreach( int id in list )
			pulseCheck( id );
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
			_ticks += PulseFrequency;
			IEnumerable<int> toCall = _pulses.Keys.ToArray();

			foreach( int id in toCall )
			{
				try
				{
					Mob m = _world.findObject( id );
					if( m == null )
					{
						bool junk;
						_pulses.TryRemove( id, out junk );
						continue;
					}
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
							world = _world
						};
						v.invoke( param );

						_world.waitForMerge();
					}
				}
				catch( Exception ex )
				{
					Log.Error( "Error executing pulse handler for {0}: {1}", id, ex );
				}
			}
		}
	}

	// World we're attached to. (It'll be a ShadowWorld.)
	World _world;

	// List of objects with heartbeat verbs.
	ConcurrentDictionary<int, bool> _pulses = new ConcurrentDictionary<int, bool>();

	// Current heartbeat tick counter.
	long _ticks = 0;

	// The actual timer we'll use.
	Timer _pulseTimer;

	// Simple semaphore to avoid stacking timer calls.
	int _pulseTimerRunning = 0;
}

}
