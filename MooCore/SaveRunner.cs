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
using System.Linq;
using System.Text;
	using System.Threading;

/// <summary>
/// Handles the world save callbacks.
/// </summary>
/// <remarks>
/// This is separated out from the CanonWorld itself, because in reality, it is
/// just another ShadowWorld consumer like anything else.
/// </remarks>
public class SaveRunner : IDisposable
{
	public SaveRunner( CanonWorld canon, WorldDatabase wdb )
	{
		_canon = canon;
		_wdb = wdb;
	}

	public void start()
	{
		Log.Info( "  Starting save callback timer" );
		_saveTimer = new Timer( (o) => saveCallback() );
		_saveTimer.Change( 30 * 1000, 30 * 1000 );
	}

	public void Dispose()
	{
		if( _saveTimer != null )
		{
			_saveTimer.Dispose();
			_saveTimer = null;
		}
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
			// Get a merge slot. Wait for it if we have to. This will prevent world
			// updates from happening until after we've saved it as it is.
			using( var token = _canon.waitMergeToken() )
			{
				try
				{
					// Delete old objects first.
					foreach( int id in _canon.emptyDeleteList() )
						_wdb.deleteMob( id );

					// Go through the remaining list and save what's there.
					foreach( int id in _canon.emptyModifiedList() )
						_wdb.saveMob( Mob.Wrap( _canon.findObject( id ) ) );
				}
				catch( Exception ex )
				{
					Log.Error( "Error during automatic save callbacks: {0}", ex );
				}
			}
		}
	}

	CanonWorld _canon;
	WorldDatabase _wdb;

	Timer _saveTimer;
	int _saveTimerRunning = 0;
}

}
