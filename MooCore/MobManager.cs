namespace Kayateia.Climoo.MooCore
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a top-level weak reference to a Mob. These are what World keeps track of.
/// This Mob may be dropped out of memory at any time. Accessing it through here will cause
/// it to be loaded again.
/// </summary>
public class MobManager
{
	public MobManager( int id, World w, WorldDatabase wdb, Mob existing )
	{
		_id = id;
		_w = w;
		_wdb = wdb;
		_mob = existing;

		// This doesn't make a lot of sense, but it keeps us from trying to GC this weak ref over and over.
		_lastUse = DateTimeOffset.UtcNow;
	}

	/// <summary>
	/// Returns the ID of the Mob we contain.
	/// </summary>
	public int id
	{
		get
		{
			return _id;
		}
	}

	/// <summary>
	/// Gets the object for use. If it's not loaded, it will be loaded. Otherwise, its use timer is reset.
	/// </summary>
	public Mob get
	{
		get
		{
			lock( _lock )
			{
				// If our mob isn't loaded, load it now.
				if( _mob == null )
				{
					_mob = _wdb.loadMob( _id, _w );
				}

				_lastUse = DateTimeOffset.UtcNow;
				return _mob;
			}
		}
	}

	/// <summary>
	/// Tries to get the mob if it's loaded, otherwise returns null.
	/// </summary>
	/// <remarks>
	/// If the mob is loaded, its last use is updated.
	/// </remarks>
	public Mob tryGet
	{
		get
		{
			if( _mob != null )
				_lastUse = DateTimeOffset.UtcNow;
			return _mob;
		}
	}

	/// <summary>
	/// This is like tryGet() (returns null if not loaded), but even weaker. The last use time is not updated.
	/// </summary>
	public Mob peek
	{
		get
		{
			return _mob;
		}
	}

	/// <summary>
	/// Returns true if the object can be removed from memory.
	/// </summary>
	public bool collectable
	{
		get
		{
			lock( _lock )
			{
				// More than 60 minutes inactive = YOU'RE FIRED!
				DateTimeOffset now = DateTimeOffset.UtcNow;
				return (now - _lastUse).TotalMinutes > 60;
			}
		}
	}

	object _lock = new object();

	int _id;
	Mob _mob;
	World _w;
	WorldDatabase _wdb;

	DateTimeOffset _lastUse;
}

}
