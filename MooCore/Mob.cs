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
using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// Implements all of the minimum functions and responsibilities of a Mob.
/// </summary>
public interface IMob
{
	/// <summary>
	/// Object's local ID, a single int like an inode number. This is auto-generated, and is used
	/// for all real internal references.
	/// </summary>
	int id { get; }

	/// <summary>
	/// Object's parent ID. This is used for object inheritance, for properties and verbs.
	/// </summary>
	int parentId { get; set; }

	/// <summary>
	/// Object's location ID. This is where the object is located, and is used for
	/// looking things up by path.
	/// </summary>
	int locationId { get; set; }

	/// <summary>
	/// ID of the owner of this object.
	/// </summary>
	int ownerId { get; set; }

	/// <summary>
	/// Access to the object's local verbs, for add, remove, and enumerate.
	/// </summary>
	void verbSet( StringI name, Verb v );
	Verb verbGet( StringI name );
	void verbDel( StringI name );
	IEnumerable<StringI> verbList { get; }

	/// <summary>
	/// Access to the object's local attributes, for add, remove, and enumerate.
	/// </summary>
	void attrSet( StringI name, TypedAttribute v );
	TypedAttribute attrGet( StringI name);
	void attrDel( StringI name );
	IEnumerable<StringI> attrList { get; }

	/// <summary>
	/// The player associated with this Mob (i.e. this Mob is its avatar).
	/// </summary>
	/// <remarks>
	/// These are pretty ephemeral -- they can be associated when the user logs in and
	/// may not be valid later. Additionally they are not stored in to the DB in anyway.
	/// </remarks>
	Player player { get; set; }

	/// <summary>
	/// The world to which this mob is attached.
	/// </summary>
	IWorld world { get; }
}

/// <summary>
/// This wrapper acts as a Mob object into which an IMob can fit glove-like.
/// </summary>
public class Mob
{
	/// <summary>
	/// Well-known attribute IDs
	/// </summary>
	public static class Attributes {
		public const string PathId = "pathid";
		public const string Name = "name";
		public const string Description = "desc";
		public const string Image = "image";		// Should be an image blob
		public const string Owner = "owner";		// Should be a mobref
		public const string Permissions = "perms";	// Int bitfield
		public const string PulseVerb = "pulseverb";
		public const string PulseFrequency = "pulsefreq";	// Should be an int
		public const string Opaque = "opaque";		// Just needs to exist
	}

	/// <summary>
	/// Path separator for addressing objects by path.
	/// </summary>
	public const char PathSep = '/';

	/// <summary>
	/// Returns the Ambiguous object, used in verb matching.
	/// </summary>
	static public Mob Ambiguous {
		get { return s_ambig; }
	}
	static Mob s_ambig = Mob.Wrap( new SpecialMob( id: -2, parentId: -3 ) );

	/// <summary>
	/// Returns the None object, used in verb matching.
	/// </summary>
	static public Mob None {
		get { return s_none; }
	}
	static Mob s_none = Mob.Wrap( new SpecialMob( id: -3, parentId: -3 ) );

	/// <summary>
	/// Returns the Any object, representing a Mob wildcard.
	/// </summary>
	static public Mob Any
	{
		get { return s_any; }
	}
	static Mob s_any = Mob.Wrap( new SpecialMob( id: -4, parentId: -3 ) );

	/// <summary>
	/// Returns the Anon object, representing anonymous users.
	/// </summary>
	static public Mob Anon
	{
		get { return s_anon; }
	}
	static Mob s_anon = Mob.Wrap( new SpecialMob( id: -5, parentId: -3 ) );

	public Mob( IMob guts )
	{
		_mob = guts;
		_world = new World( _mob.world );
	}

	/// <summary>
	/// Simple wrapper for creating a Mob object, to use its extra methods.
	/// </summary>
	static public Mob Wrap( IMob mob )
	{
		return new Mob( mob );
	}

	/// <summary>
	/// Returns the underlying IMob.
	/// </summary>
	public IMob get
	{
		get
		{
			return _mob;
		}
	}


	/// <summary>
	/// Mob reference -- weak reference good for attributes and persistence.
	/// </summary>
	[DataContract( Namespace="Climoo", Name="MobRef" )]
	public class Ref {
		public Ref( Mob m )
		{
			_id = m.id;
		}

		public Ref( IMob m )
		{
			_id = m.id;
		}

		public Ref( int id )
		{
			_id = id;
		}

		/// <summary>
		/// This is to be used only for persistence.
		/// </summary>
		public Ref()
		{
		}

		[DataMember]
		public int id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public override string ToString()
		{
			return "<#{0}>".FormatI(_id );
		}

		int _id;
	}

	/////////////////////////////////////////////////////////////////////////////
	// These are pass-throughs for IMob.
	public int id { get { return _mob.id; } }

	public int parentId
	{
		get { return _mob.parentId; }
		set { _mob.parentId = value; }
	}

	public int locationId
	{
		get { return _mob.locationId; }
		set { _mob.locationId = value; }
	}

	public int ownerId
	{
		get { return _mob.ownerId; }
		set { _mob.ownerId = value; }
	}

	public Perm[] permissions
	{
		get
		{
			TypedAttribute ta = attrGet( Attributes.Permissions );
			if( ta != null )
				return (Perm[])ta.contents;
			else
				return null;
		}

		set
		{
			attrSet( Attributes.Permissions, TypedAttribute.FromValue( value ) );
		}
	}

	public bool verbHas( StringI name ) { return _mob.verbGet( name ) != null; }
	public void verbSet( StringI name, Verb v ) { _mob.verbSet( name, v ); }
	public Verb verbGet( StringI name ) { return _mob.verbGet( name ); }
	public void verbDel( StringI name ) { _mob.verbDel( name ); }
	public IEnumerable<StringI> verbList { get { return _mob.verbList; } }

	public bool attrHas( StringI name ) { return _mob.attrGet( name ) != null; }
	public void attrSet( StringI name, TypedAttribute v ) { _mob.attrSet( name, v ); }
	public void attrSet( StringI name, string v ) { attrSet( name, TypedAttribute.FromValue( v ) ); }
	public TypedAttribute attrGet( StringI name ) { return _mob.attrGet( name ); }
	public void attrDel( StringI name ) { _mob.attrDel( name ); }
	public IEnumerable<StringI> attrList { get { return _mob.attrList; } }

	public Player player
	{
		get { return _mob.player; }
		set { _mob.player = value; }
	}

	public World world
	{
		get { return new World( _mob.world ); }
	}

	/////////////////////////////////////////////////////////////////////////////

	public Mob parent
	{
		get
		{
			if( this.parentId >= 0 )
				return _world.findObject( this.parentId );
			else
				return null;
		}
	}

	public Mob location
	{
		get
		{
			if( this.locationId >= 0 )
				return _world.findObject( this.locationId );
			else
				return null;
		}
	}

	public Mob owner
	{
		get
		{
			if( this.ownerId >= 0 )
				return _world.findObject( this.ownerId );
			else
				return null;
		}
	}

	// Convenience get/set for a few common attributes.
	public string name
	{
		get { return NullOrStr( findAttribute( Attributes.Name, true ) ); }
		set
		{
			attrSet( Attributes.Name, TypedAttribute.FromValue( value ) );
		}
	}
	public string desc
	{
		get { return NullOrStr( findAttribute( Attributes.Description, true ) ); }
		set
		{
			attrSet( Attributes.Description, TypedAttribute.FromValue( value ) );
		}
	}
	public string pathId
	{
		get { return NullOrStr( findAttribute( Attributes.PathId, true ) ); }
		set
		{
			attrSet( Attributes.PathId, TypedAttribute.FromValue( value ) );
		}
	}
	public string pulseVerb
	{
		get
		{
			return NullOrStr( findAttribute( Attributes.PulseVerb, true ) );
		}
	}
	public int pulseFreq
	{
		get
		{
			return NullOrZero( findAttribute( Attributes.PulseFrequency, true ) );
		}
	}

	static int NullOrZero( TypedAttribute attr )
	{
		if( attr == null )
			return 0;
		else
			return attr.getContents<int>();
	}
	static string NullOrStr( TypedAttribute attr )
	{
		if( attr == null )
			return null;
		else
			return attr.str;
	}

	/// <summary>
	/// Returns a read-only collection of all the available attributes to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<TypedAttribute>> allAttrs
	{
		get
		{
			var list = new Dictionary<StringI, SourcedItem<TypedAttribute>>();
			traverseInheritance( null, ( mob ) =>
			{
				// Replace any in the list with local ones.
				foreach( var item in mob.attrList )
					list[item] = new SourcedItem<TypedAttribute>( mob, item, mob.attrGet( item ) );
			} );

			return list;
		}
	}

	/// <summary>
	/// Returns a read-only collection of all the available verbs to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<Verb>> allVerbs
	{
		get
		{
			var list = new Dictionary<StringI, SourcedItem<Verb>>();
			traverseInheritance( null, ( mob ) =>
			{
				// Replace any in the list with local ones.
				foreach( var item in mob.verbList )
					list[item] = new SourcedItem<Verb>( mob, item, mob.verbGet( item ) );
			} );
			return list;
		}
	}

	// Generic tree traversal for looking for something through inheritance.
	T traverseInheritance<T>( Func<Mob, T> finder ) where T : class
	{
		var rv = finder( this );
		if( rv != null )
			return rv;
		else
		{
			if( _mob.parentId > 0 )
			{
				if( _mob.parentId == _mob.id )
					throw new InvalidOperationException( "Mob is its own parent; call out the time squad" );
				Mob parentMob = _world.findObject( _mob.parentId );
				if( parentMob != null )
				{
					return parentMob.traverseInheritance( finder );
				}
				else
				{
					// FIXME: Invalid parent. Log or something.
					return null;
				}
			}
			else
				return null;
		}
	}

	// Generic tree traversal, non-search.
	void traverseInheritance( Action<Mob> visitorPre, Action<Mob> visitorPost )
	{
		if( visitorPre != null )
			visitorPre( this );

		if( _mob.parentId > 0 )
		{
			if( _mob.parentId == _mob.id )
				throw new InvalidOperationException( "Mob is its own parent; call out the time squad" );
			Mob parentMob = _world.findObject( _mob.parentId );
			if( parentMob != null )
			{
				parentMob.traverseInheritance( visitorPre, visitorPost );
			}
		}

		if( visitorPost != null )
			visitorPost( this );
	}

	/// <summary>
	/// Looks for a named verb on us, or on a parent object.
	/// </summary>
	/// <param name="verb">The verb name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A Verb object for the verb, or null if not found.</returns>
	public Verb findVerb( string verb, bool localOnly = false )
	{
		return traverseInheritance( ( obj ) => obj.verbGet( verb ) );
	}

	/// <summary>
	/// Looks for a named attribute on us, or on a parent object.
	/// </summary>
	/// <param name="name">The attribute name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A TypedAttribute with the attribute's contents, or null if not found.</returns>
	public TypedAttribute findAttribute( string name, bool localOnly = false )
	{
		return traverseInheritance( ( obj ) => obj.attrGet( name ) );
	}

	/// <summary>
	/// Returns a fully qualified path name, which may be used in a more human-friendly
	/// manner to find certain named objects within a hierarchy of objects.
	/// </summary>
	public string fqpn
	{
		get
		{
			// Find my path component.
			string me;
			if( !attrHas( Attributes.PathId ) || attrGet( Attributes.PathId ).str.IsNullOrEmpty() || _mob.locationId <= 0 )
			{
				if( _mob.id == 1 )
					me = "";
				else
					me = StringCase.FormatI( "#{0}", _mob.id );
			}
			else
			{
				// Put my path name on the back.
				me = attrGet( Attributes.PathId ).str;

				// Add our location's path.
				Mob locMob = _world.findObject( _mob.locationId );
				if( locMob != null )
				{
					me = StringCase.FormatI( "{0}{1}{2}", locMob.fqpn, PathSep, me );
				}
				else
				{
					// FIXME: Invalid parent. Log or something.
					me = StringCase.FormatI( "#{0}", _mob.id );
				}
			}

			return me;
		}
	}

	public IEnumerable<Mob> contained
	{
		get
		{
			int id = this.id;
			return _world.findObjects( ( m ) => m.locationId == id );
		}
	}

	public bool isDescendentOf( int id )
	{
		return traverseInheritance( ( m ) => m.id == id ? "" : null ) != null;
	}

	World _world;
	IMob _mob;
}

}
