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
using System.Linq;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Implements the guts of the canonical mobs that actually track all the info.
/// </summary>
/// <remarks>
/// No real locking is done in here (just enough to ensure the integrity of the data
/// structures themselves). The assumption is that all accesses to this object will be
/// read-only, or they'll happen in a place where the whole world is effectively locked.
/// 
/// IMob is implemented here as a convenience for WorldDatabase.
/// </remarks>
public class CanonMob : IMob
{
	internal CanonMob( CanonWorld world, int id )
	{
		_world = world;
		_id = id;
		_parentId = new Timestamped<int>( 1 );
		_ownerId = new Timestamped<int>( Mob.None.id );
		_locationId = new Timestamped<int>( 1 );
		_perms = new Timestamped<Perm>( Perm.R | Perm.F );
	}

	public int id
	{
		get
		{
			return _id;
		}
	}

	// The MO for all of these is basically the same: they return timestamped copies of
	// the values, and when you try to set one, it only works if the new value is newer than the
	// existing value.
	public Timestamped<int> parentId
	{
		get
		{
			return _parentId;
		}
		set
		{
			if( value.stamp > _parentId.stamp )
			{
				_parentId = value;
				_modified = true;
			}
		}
	}

	public Timestamped<int> locationId
	{
		get
		{
			return _locationId;
		}
		set
		{
			if( value.stamp > _locationId.stamp )
			{
				_locationId = value;
				_modified = true;
			}
		}
	}

	public Timestamped<int> ownerId
	{
		get
		{
			return _ownerId;
		}
		set
		{
			if( value.stamp > _ownerId.stamp )
			{
				_ownerId = value;
				_modified = true;
			}
		}
	}

	public Timestamped<Perm> perms
	{
		get
		{
			return _perms;
		}
		set
		{
			if( value.get & ~(Perm.R | Perm.W | Perm.F | Perm.Coder | Perm.Mayor | Perm.Player) )
				throw new InvalidOperationException( "Only R, W, F, Coder, Mayor, and Player permissions are valid for mobs" );
			if( value.stamp > _perms.stamp )
			{
				_perms = value;
				_modified = true;
			}
		}
	}

	// Verbs and attributes are a slightly more special case. We have to actually keep track of deletions
	// with a timestamp as well as what's actually there, because someone might have deleted the verb
	// after someone else added it.
	class KeyValueStore<T>
	{
		// Set present, then remove deleted.
		public void set( StringI name, Timestamped<T> v, ref bool modified )
		{
			Timestamped<T> val;
			if( _contained.TryGetValue( name, out val ) )
			{
				if( v.stamp > val.stamp )
				{
					_contained[name] = v;
					modified = true;
				}
			}
			else
			{
				_contained[name] = v;
				modified = true;
			}

			Timestamped<bool> junk;
			_deleted.TryRemove( name, out junk );
			modified = true;
		}

		// Check deleted, then present.
		public Timestamped<T> get( StringI name )
		{
			Timestamped<bool> junk;
			if( _deleted.TryGetValue( name, out junk ) )
				return new Timestamped<T>( default( T ), junk.stamp );

			Timestamped<T> val;
			if( _contained.TryGetValue( name, out val ) )
				return val;
			else
				return null;
		}

		// Remove present, then delete.
		public void del( StringI name )
		{
			Timestamped<T> val;
			if( _contained.TryRemove( name, out val ) )
				_deleted[name] = new Timestamped<bool>( true );
		}

		// Grab the list of present items, and merge in deleted ones as deleted.
		public IDictionary<StringI, Timestamped<T>> list
		{
			get
			{
				var items = new Dictionary<StringI, Timestamped<T>>( _contained );
				foreach( var kp in _deleted )
					items[kp.Key] = new Timestamped<T>( default( T ), kp.Value.stamp );
				return items;
			}
		}

		ConcurrentDictionary<StringI, Timestamped<T>> _contained = new ConcurrentDictionary<StringI, Timestamped<T>>();
		ConcurrentDictionary<StringI, Timestamped<bool>> _deleted = new ConcurrentDictionary<StringI, Timestamped<bool>>();
	}

	public void verbSet( StringI name, Timestamped<Verb> v )
	{
		// If there's an old verb, copy over permissions and make
		// it appear that we just replaced the contained value.
		Timestamped<Verb> old = _verbs.get( name );
		if( old != null && old.get != null )
			v.get.perms = old.get.perms;

		_verbs.set( name, v, ref _modified );
	}

	public Timestamped<Verb> verbGet( StringI name )
	{
		return _verbs.get( name );
	}

	public void verbDel( StringI name )
	{
		_verbs.del( name );
	}

	public IDictionary<StringI, Timestamped<Verb>> verbList
	{
		get
		{
			return _verbs.list;
		}
	}

	public void attrSet( StringI name, Timestamped<TypedAttribute> v )
	{
		// If there's an old attribute, copy over permissions and make
		// it appear that we just replaced the contained value.
		Timestamped<TypedAttribute> old = _attrs.get( name );
		if( old != null && old.get != null )
			v.get.perms = old.get.perms;

		_attrs.set( name, v, ref _modified );
	}

	public Timestamped<TypedAttribute> attrGet( StringI name )
	{
		return _attrs.get( name );
	}

	public void attrDel( StringI name )
	{
		_attrs.del( name );
	}

	public IDictionary<StringI, Timestamped<TypedAttribute>> attrList
	{
		get
		{
			return _attrs.list;
		}
	}

	// This is not something we expect a lot of contention on.
	public Player player
	{
		get
		{
			return _player;
		}
		set
		{
			// We don't set _modified here because it's essentially an in-game-only thing.
			_player = value;
		}
	}

	/// <summary>
	/// Returns true if the mob has changed since the last check. The flag is reset.
	/// </summary>
	public bool resetModified()
	{
		bool result = _modified;
		_modified = false;
		return result;
	}

	// These are here largely to make world database saving and unit tests easier. Not
	// recommended for normal usage.
	#region IMob Members
	int IMob.id
	{
		get
		{
			return this.id;
		}
	}

	int IMob.parentId
	{
		get
		{
			return this.parentId.get;
		}
		set
		{
			this.parentId = new Timestamped<int>( value );
		}
	}

	int IMob.locationId
	{
		get
		{
			return this.locationId.get;
		}
		set
		{
			this.locationId = new Timestamped<int>( value );
		}
	}

	int IMob.ownerId
	{
		get
		{
			return this.ownerId.get;
		}
		set
		{
			this.ownerId = new Timestamped<int>( value );
		}
	}

	Perm IMob.perms
	{
		get
		{
			return this.perms.get;
		}
		set
		{
			this.perms = new Timestamped<Perm>( value );
		}
	}

	void IMob.verbSet( StringI name, Verb v )
	{
		verbSet( name, new Timestamped<Verb>( v ) );
	}

	Verb IMob.verbGet( StringI name )
	{
		var v = verbGet( name );
		if( v != null )
			return v.get;
		else
			return null;
	}

	void IMob.verbDel( StringI name )
	{
		verbDel( name );
	}

	IEnumerable<StringI> IMob.verbList
	{
		get
		{
			return verbList.Select( v => v.Key );
		}
	}

	void IMob.attrSet( StringI name, TypedAttribute v )
	{
		attrSet( name, new Timestamped<TypedAttribute>( v ) );
		if( name == Mob.Attributes.PulseFrequency )
			_world.pulseCheck( this );
	}

	TypedAttribute IMob.attrGet( StringI name )
	{
		var a = attrGet( name );
		if( a != null )
			return a.get;
		else
			return null;
	}

	void IMob.attrDel( StringI name )
	{
		attrDel( name );
	}

	IEnumerable<StringI> IMob.attrList
	{
		get
		{
			return attrList.Select( a => a.Key );
		}
	}

	Player IMob.player
	{
		get
		{
			return this.player;
		}
		set
		{
			this.player = value;
		}
	}

	IWorld IMob.world
	{
		get
		{
			return _world;
		}
	}
	#endregion

	// The reality we belong to.
	CanonWorld _world;

	// IDs
	int _id;

	// Parent ID (local only)
	Timestamped<int> _parentId;

	// Location ID (local only)
	Timestamped<int> _locationId;

	// Permissions mask (local only)
	Timestamped<Perm> _perms;

	// Object owner (local only)
	Timestamped<int> _ownerId;

	// Verbs attached to the object
	KeyValueStore<Verb> _verbs = new KeyValueStore<Verb>();

	// Attributes on the object
	KeyValueStore<TypedAttribute> _attrs = new KeyValueStore<TypedAttribute>();

	// The player we're attached to, if any.
	Player _player;

	// True if we've been modified since the last check.
	bool _modified = false;
}

}
