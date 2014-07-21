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
using System.Runtime.Serialization;
using System.Text;

/// <summary>
/// Represents an abstract permission (ACL) granted to an actor to perform an action, OR
/// an attempted access by an actor (in which case only a single bit should be set.)
/// </summary>
[DataContract( Namespace="Climoo", Name="Perm" )]
public class Perm
{
	public Perm()
	{
		this.perms = new PermBits();
	}

	/// <summary>
	/// The actor the permission is about.
	/// </summary>
	[DataMember]
	public int actorId { get; set; }

	/// <summary>
	/// Whether this is an allow or deny permission.
	/// </summary>
	public Type type { get; set; }
	public enum Type
	{
		Allow,
		Deny
	}

	// Ghost member for serialization. Otherwise we get 0/1.
	[DataMember( Name="type" )]
	public string typeString
	{
		get
		{
			return this.type.ToStringI();
		}

		set
		{
			Type t;
			if( Enum.TryParse<Type>( value, out t ) )
				this.type = t;
		}
	}

	/// <summary>
	/// The bitmask of what's being allowed or denied.
	/// </summary>
	public PermBits perms { get; set; }

	// We actually serialize this one for simplicity's sake.
	[DataMember]
	public int permBits
	{
		get
		{
			return this.perms.mask;
		}
		set
		{
			// Somehow we can get here without the constructor having been called (!?) so
			// we have to fill this in on the fly.
			if( this.perms == null )
				this.perms = new PermBits();
			this.perms.mask = value;
		}
	}

	/// <summary>
	/// Specific item (verb/attribute) if desired.
	/// </summary>
	public StringI specific { get; set; }

	[DataMember]
	public string specificString
	{
		get
		{
			return this.specific;
		}
		set
		{
			this.specific = value;
		}
	}

	public override string ToString()
	{
		var names = new List<string>();
		for( int i=0; i<31; ++i )
		{
			int mask = 1 << i;
			if( this.perms & mask )
			{
				string name = PermBits.PermNames[mask];
				names.Add( name );
			}
		}
		string bitString = String.Join( "/", names );

		string actor = this.actorId == Mob.Any.id ? "#Any" : "#{0}".FormatI( this.actorId );

		return CultureFree.Format( "<Perm: {0} {2} to #{1}{3}>",
			this.type, actor, bitString, this.specific != null ? " on "+this.specific : new StringI( "" ) );
	}

	/// <summary>
	/// Treats this object as an action question and matches it against the list of
	/// permissions given as a parameter.
	/// </summary>
	/// <remarks>This may throw if things aren't right (permissions attribute isn't Perm[], etc)</remarks>
	/// <returns>True if it's allowed.</returns>
	public bool check( Mob target )
	{
		// Make sure we have everything we need for a proper question.
		if( (this.perms & PermBits.Verb) || (this.perms & PermBits.Attr) )
			if( String.IsNullOrEmpty( this.specific ) )
				throw new ArgumentException( "Permission question has verb or attribute, but no specific name." );
		if( this.perms == 0 )
			throw new ArgumentException( "Permission question has no bits set." );
		if( (this.perms.mask & (this.perms.mask - 1)) != 0 )
			throw new ArgumentException( "Permission question has more than one bit." );
		if( this.perms & PermBits.AO )
			throw new ArgumentException( "Permission question can't specify AO." );
		// Post-condition: We have a proper single bit permission question with a specific if appropriate.

		// Team members can do anything they please.
		Mob actor = target.world.findObject( this.actorId );
		if( actor.teamMember )
			return true;

		// If we're editing the permissions attribute itself, only team members can do that.
		if( (this.perms & PermBits.AW) && this.specific == Mob.Attributes.Permissions )
			return false;

		// Get the permissions assertions, starting with the specific target mob and working up the inheritance chain.
		var permissions = new List<KeyValuePair<Mob, Perm[]>>();
		for( Mob m = target; m != null; m = m.parent )
		{
			var permattr = m.attrGet( Mob.Attributes.Permissions );
			if( permattr != null )
			{
				Perm[] tp = (Perm[])permattr.contents;
				permissions.Add( new KeyValuePair<Mob, Perm[]>( m, tp ) );
			}
		}

		// Are we dealing with an attribute? If so, look for a sticky bit starting at the bottom.
		if( this.perms & PermBits.Attr )
		{
			// This is not so efficient, but it should be okay for the small numbers we're working with.
			for( int i = permissions.Count - 1; i > 0; --i )
			{
				var kvp = permissions[i];
				if( kvp.Value.Any( (p) => p.specific == this.specific && p.perms & PermBits.AO ) )
				{
					// We really want to do this check with this other object's permissions.
					return check( kvp.Key );
				}
			}
		}

		// If it's not an AO attribute, then the owner is allowed to do anything.
		if( target.ownerId == this.actorId )
			return true;

		// Find all the mob IDs in the inheritance chain for this question's actor ID. We
		// stop before #1 because it probably has all sorts of powers.
		var actors = new List<int>();
		if( this.actorId == 1 )
			actors.Add( 1 );
		else
		{
			for( Mob m = actor; m.id != 1; m = m.parent )
				actors.Add( m.id );
		}

		// If we find nothing, we have to fall back to looking for an "opaque" attribute
		// to determine what the base permissions are. So we'll just do that now and tack the defaults
		// onto the end.
		if( target.findAttribute( Mob.Attributes.Opaque ) == null )
		{
			// Not opaque: give a set of default permissions that lets people read most of it.
			var clearPerms = new Perm[]
			{
				new Perm()
				{
					actorId = Mob.Any.id,
					perms = PermBits.AR | PermBits.OR | PermBits.OF | PermBits.VR,
					specific = null
				}
			};
			permissions.Add( new KeyValuePair<Mob,Perm[]>( target, clearPerms ) );
		}

		// Go through until we find a permission that matches what we're looking for.
		foreach( var kvp in permissions )
		{
			foreach( var p in kvp.Value )
			{
				if( p.actorId == Mob.Any.id || actors.Any( a => a == p.actorId ) )
				{
					// Make sure we're talking about the same subject.
					if( matches( p ) )
						return p.type == Type.Allow;
				}
			}
		}

		// Didn't find anything: denied.
		return false;
	}

	/// <summary>
	/// Version of check() throws a PermissionException if it fails its check.
	/// </summary>
	public void checkOrThrow( Mob target, string message )
	{
		if( !check( target ) )
			throw new Exceptions.PermissionFailure( message, this );
	}

	/// <summary>
	/// Treats us as an action and matches us against the specified permission.
	/// Returns true if the permission matches.
	/// </summary>
	public bool matches( Perm other )
	{
		// Specifics might be null, but they must still match. Any bits that are shared
		// make the permission relevant.
		return (this.perms & other.perms)
			&& (other.specific == null || this.specific == other.specific);
	}

	// These are some common permissions we need to test against.
	static public Perm AttrRead( int actorId, string specific )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.AR, specific = specific };
	}
	static public Perm AttrWrite( int actorId, string specific )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.AW, specific = specific };
	}
	static public Perm VerbRead( int actorId, string specific )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.VR, specific = specific };
	}
	static public Perm VerbWrite( int actorId, string specific )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.VW, specific = specific };
	}
	static public Perm ObjRead( int actorId )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.OR };
	}
	static public Perm ObjWrite( int actorId )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.OW };
	}
	static public Perm ObjMove( int actorId )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.OM };
	}
	static public Perm ObjFertile( int actorId )
	{
		return new Perm() { actorId = actorId, permBits = PermBits.OF };
	}
}

/// <summary>
/// Represents a permission bitmask.
/// </summary>
public class PermBits {
											// Permission					/ Permission Question
	public const int AR = 1 << 0;			// Read [attribute]
	public const int AW = 1 << 1;			// Write [attribute]
	public const int AO = 1 << 2;			// Ownership sticky [attribute]	/ Invalid
	public const int VR = 1 << 3;			// Read [verb]
	public const int VW = 1 << 4;			// Write [verb]
	public const int VO = 1 << 5;			// Ownership sticky [verb]		/ Invalid
	public const int OR = 1 << 6;			// Read [object]
	public const int OW = 1 << 7;			// Write [object]
	public const int OM = 1 << 8;			// Move [object]
	public const int OF = 1 << 9;			// Fertile [object]				/ Can inherit

	// Composite bits for the various major types.
	public const int Attr = AR | AW | AO;
	public const int Verb = VR | VW;
	public const int Obj = OR | OW | OM | OF;

	public PermBits()
	{
		_mask = 0;
	}

	public PermBits( int mask )
	{
		_mask = mask;
	}

	static public Dictionary<int, string> PermNames = new Dictionary<int,string> {
		{ AR, "AR" },
		{ AW, "AW" },
		{ AO, "AO" },
		{ VR, "VR" },
		{ VW, "VW" },
		{ VO, "VO" },
		{ OR, "OR" },
		{ OW, "OW" },
		{ OM, "OM" },
		{ OF, "OF" }
	};

	/// <summary>
	/// Returns true if the specified permissions contain any of the masked bits.
	/// </summary>
	public bool have( PermBits mask )
	{
		return (this.mask & mask.mask) != 0;
	}

	public int mask {
		get { return _mask; }
		set { _mask = value; }
	}

	static public implicit operator PermBits( int m ) { return new PermBits(m); }
	static public implicit operator int( PermBits p ) { return p._mask; }
	static public PermBits operator ~( PermBits p ) { return new PermBits( ~p.mask ); }
	static public PermBits operator |( PermBits a, PermBits b ) { return new PermBits( a.mask | b.mask ); }
	static public PermBits operator |( PermBits a, int b ) { return new PermBits( a.mask | b ); }
	static public bool operator &( PermBits a, PermBits b ) { return a.have( b.mask ); }
	static public bool operator &( PermBits a, int b ) { return a.have( b ); }

	int _mask;
}

}
