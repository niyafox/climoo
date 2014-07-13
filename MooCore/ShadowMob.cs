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

/// <summary>
/// Implements Mobs for the "shadow universe" concept for copy-on-write, aka MOOCOW.
/// </summary>
/// <remarks>
/// Most of what goes on in here is literal COW operations, so there's not much point in
/// spending a lot of time commenting it all. There are some comments where things are
/// pertinently explainable.
/// </remarks>
public class ShadowMob : IMob
{
	public ShadowMob( ShadowWorld world, CanonMob canonMob )
	{
		_world = world;
		_canon = canonMob;
		_verbs = new VerbStore( this, _canon );
		_attrs = new AttrStore( this, _canon );
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// We use a dictionary instead of a real queue here because generally, it doesn't matter
	// what order things are written in, and we also only care about the latest update.
	void enq( string key, Action<CanonMob> action )
	{
		// Enqueue the change.
		_actions[key] = new Timestamped<Action<CanonMob>>( action );

		// If we have a clear signal on the canon world, take a shot.
		_world.tryMerge( this );
	}

	// Returns the canon value or the local (shadow) value, whichever is newest.
	T valueOrShadow<T>( ref Timestamped<T> shadow, Func<Timestamped<T>> getValue )
	{
		var canonVal = getValue();

		// If we have no shadow value, *or* our shadow value is newer, use that.
		// If we *do* have a shadow value, and it's older, nuke it and use the canon.
		if( shadow != null )
		{
			if( shadow.stamp > canonVal.stamp )
			{
				return shadow.get;
			}
			else
				shadow = null;
		}

		// Otherwise, use the canon.
		return canonVal.get;
	}

	/// <summary>
	/// Returns the action queue and empties it.
	/// </summary>
	public IEnumerable<Action<CanonMob>> emptyActionQueue()
	{
		var rv = _actions.Values.Select( a => a.get ).ToArray();
		_actions.Clear();
		return rv;
	}

	/// <summary>
	/// For use by ShadowWorld.
	/// </summary>
	public CanonMob canon
	{
		get
		{
			return _canon;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////

	public int id
	{
		get
		{
			return _canon.id;
		}
	}

	public int parentId
	{
		get
		{
			return valueOrShadow( ref _parentId, () => _canon.parentId );
		}
		set
		{
			var newval = new Timestamped<int>( value );
			_parentId = newval;
			enq( "parentId", (m) => { m.parentId = newval; _parentId = null; } );
		}
	}
	Timestamped<int> _parentId;

	public int locationId
	{
		get
		{
			return valueOrShadow( ref _locationId, () => _canon.locationId );
		}
		set
		{
			var newval = new Timestamped<int>( value );
			_locationId = newval;
			enq( "locationId", (m) => { m.locationId = newval; _locationId = null; } );
		}
	}
	Timestamped<int> _locationId;

	public int ownerId
	{
		get
		{
			return valueOrShadow( ref _ownerId, () => _canon.ownerId );
		}
		set
		{
			var newval = new Timestamped<int>( value );
			_ownerId = newval;
			enq( "ownerId", (m) => { m.ownerId = newval; _ownerId = null; } );
		}
	}
	Timestamped<int> _ownerId;

	public Perm perms
	{
		get
		{
			return valueOrShadow( ref _perms, () => _canon.perms );
		}
		set
		{
			var newval = new Timestamped<Perm>( value );
			_perms = newval;
			enq( "perms", (m) => { m.perms = newval; _perms = null; } );
		}
	}
	Timestamped<Perm> _perms;

	//////////////////////////////////////////////////////////////////////////////////////////////

	abstract class KeyValueStore<T>
		where T : new()
	{
		protected abstract Timestamped<T> canonGet( StringI n );
		protected abstract void canonDel( StringI n );
		protected abstract void canonSet( StringI n, Timestamped<T> v );
		protected abstract IDictionary<StringI, Timestamped<T>> canonList();

		public void set( StringI name, T v )
		{
			Timestamped<T> tv = new Timestamped<T>( v );
			_contained[name] = tv;
			canonSet( name, tv );
		}

		// Pull the canon value. If we have a local value, check its timestamp.
		// If our timestamp is newer, use ours. Otherwise, nuke ours and use canon.
		public T get( StringI name )
		{
			Timestamped<T> canon = canonGet( name );
			Timestamped<T> ours;
			if( _contained.TryGetValue( name, out ours ) )
			{
				if( canon == null )
					return ours.get;
				else
				{
					if( ours.stamp > canon.stamp )
						return ours.get;
					else
					{
						// Our value is no good. Fall through to the end return.
						_contained.Remove( name );
					}
				}
			}

			if( canon != null )
				return canon.get;
			else
				return default( T );
		}

		public void del( StringI name )
		{
			// Remove it from here either way.
			_contained.Remove( name );

			// If there is a canon item, then mark it for deletion.
			Timestamped<T> canon = canonGet( name );
			if( canon.get != null )
			{
				_deleted[name] = new Timestamped<bool>( true );
				canonDel( name );
			}
		}

		public IEnumerable<StringI> list
		{
			get
			{
				// Pull the canon list.
				var all = new Dictionary<StringI, Timestamped<T>>( canonList() );

				// Mix in anything we have that might be newer.
				foreach( var kp in _contained )
					if( !all.ContainsKey( kp.Key ) || _contained[kp.Key].stamp > all[kp.Key].stamp )
						all[kp.Key] = kp.Value;
				foreach( var kp in _deleted )
					if( all.ContainsKey( kp.Key ) && _deleted[kp.Key].stamp > all[kp.Key].stamp )
						all.Remove( kp.Key );

				// Return the list of verb names minus anything deleted.
				return all.Where( kp => kp.Value.get != null ).Select( kp => kp.Key );
			}
		}

		protected Dictionary<StringI, Timestamped<T>> _contained = new Dictionary<StringI,Timestamped<T>>();
		protected Dictionary<StringI, Timestamped<bool>> _deleted = new Dictionary<StringI,Timestamped<bool>>();
	}

	class VerbStore : KeyValueStore<Verb>
	{
		public VerbStore( ShadowMob us, CanonMob m )
		{
			_us = us;
			_cm = m;
		}

		protected override Timestamped<Verb> canonGet( StringI n )
		{
			return _cm.verbGet( n );
		}

		protected override void canonDel( StringI n )
		{
			_us.enq( "verb" + n, (m) =>
			{
				m.verbDel( n );
				_contained.Remove( n );
				_deleted.Remove( n );
			} );
		}

		protected override void canonSet( StringI n, Timestamped<Verb> v )
		{
			_us.enq( "verb" + n, (m) =>
			{
				m.verbSet( n, v );
				_contained.Remove( n );
				_deleted.Remove( n );
			} );
		}

		protected override IDictionary<StringI, Timestamped<Verb>> canonList()
		{
			return _cm.verbList;
		}

		ShadowMob _us;
		CanonMob _cm;
	}

	public void verbSet( StringI name, Verb v ) { _verbs.set( name, v ); }
	public Verb verbGet( StringI name ) { return _verbs.get( name ); }
	public void verbDel( StringI name ) { _verbs.del( name ); }
	public IEnumerable<StringI> verbList { get { return _verbs.list; } }

	//////////////////////////////////////////////////////////////////////////////////////////////

	class AttrStore : KeyValueStore<TypedAttribute>
	{
		public AttrStore( ShadowMob us, CanonMob m )
		{
			_us = us;
			_cm = m;
		}

		protected override Timestamped<TypedAttribute> canonGet( StringI n )
		{
			return _cm.attrGet( n );
		}

		protected override void canonDel( StringI n )
		{
			_us.enq( "attr" + n, (m) =>
			{
				m.attrDel( n );
				_contained.Remove( n );
				_deleted.Remove( n );
			} );
		}

		protected override void canonSet( StringI n, Timestamped<TypedAttribute> v )
		{
			_us.enq( "attr" + n, (m) =>
			{
				m.attrSet( n, v );
				_contained.Remove( n );
				_deleted.Remove( n );
			} );
		}

		protected override IDictionary<StringI, Timestamped<TypedAttribute>> canonList()
		{
			return _cm.attrList;
		}

		ShadowMob _us;
		CanonMob _cm;
	}

	public void attrSet( StringI name, TypedAttribute v ) { _attrs.set( name, v ); }
	public TypedAttribute attrGet( StringI name ) { return _attrs.get( name ); }
	public void attrDel( StringI name ) { _attrs.del( name ); }
	public IEnumerable<StringI> attrList { get { return _attrs.list; } }

	//////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// The player associated with this Mob (i.e. this Mob is its avatar).
	/// </summary>
	/// <remarks>
	/// These are pretty ephemeral -- they can be associated when the user logs in and
	/// may not be valid later. Additionally they are not stored in to the DB in anyway.
	/// </remarks>
	public Player player
	{
		get
		{
			//if( _player != null )
			//	return _player;
			//else
				return _canon.player;
		}
		set
		{
			_canon.player = value;
		//	_player = value;
		//	enq( "player", (m) => { m.player = value; _player = null; } );
		}
	}
	//Player _player;


	public IWorld world
	{
		get
		{
			return _world;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////

	Dictionary<string, Timestamped<Action<CanonMob>>> _actions = new Dictionary<string,Timestamped<Action<CanonMob>>>();
	ShadowWorld _world;
	CanonMob _canon;

	VerbStore _verbs;
	AttrStore _attrs;
}

}
