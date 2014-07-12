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
/// Implements all of the functions and responsibilities of a World: a managed collection of objects.
/// </summary>
public interface IWorld
{
	/// <summary>
	/// Returns the number of ticks that have passed since the world loaded. These
	/// will increase at one per second no matter how frequently the timer happens.
	/// </summary>
	long ticks { get; }

	/// <summary>
	/// This delegate must return valid URLs for attribute data. This lets us produce web
	/// links for things like images.
	/// </summary>
	World.UrlGenerator attributeUrlGenerator { get; }

	/// <summary>
	/// Gets the list of checkpoints from the world database and passes on the savings.
	/// </summary>
	WorldCheckpoint[] checkpoints { get; }

	/// <summary>
	/// Performs a checkpoint in the world database.
	/// </summary>
	void checkpoint( string name );

	/// <summary>
	/// Performs a checkpoint in the world database.
	/// </summary>
	void checkpointRemove( ulong id );

	/// <summary>
	/// Basic logic for creating objects in the world database.
	/// </summary>
	/// <remarks>
	/// This is very raw and any mob created here must have quite a bit of work done to it before it's final.
	/// </remarks>
	IMob createObject();

	/// <summary>
	/// Locates an existing mob by ID.
	/// </summary>
	IMob findObject( int id );

	/// <summary>
	/// Locates many objects by search predicate.
	/// </summary>
	/// <remarks>
	/// The IMobs during the predicate are ephemeral. Please do not try to modify them.
	/// </remarks>
	IEnumerable<IMob> findObjects( Func<IMob, bool> predicate );

	/// <summary>
	/// Destroys an object.
	/// </summary>
	void destroyObject( int id );
}

/// <summary>
/// This wrapper acts as a World object into which an IWorld can fit glove-like.
/// </summary>
public class World
{
	public World( IWorld guts )
	{
		_world = guts;
	}

	/// <summary>
	/// Constants for special (well-known) object paths.
	/// </summary>
	public class WellKnownObjects
	{
		public const string Root = "/";
		public const string Player = "/templates/player";
		public const string Room = "/templates/room";
		public const string Portal = "/templates/portal";
	}

	public const string ConfigNextId = "nextid";

	public delegate string UrlGenerator(Mob obj, string name);

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
			newMob.attrSet( item.Name, TypedAttribute.FromValue( item.Value ) );
		if( location.HasValue )
			newMob.locationId = location.Value;

		// Objects are parented onto the PTB by default. Otherwise...
		if( parent.HasValue )
			newMob.parentId = parent.Value;

		return newMob;
	}

	/// <summary>
	/// Locates an object by search predicate.
	/// </summary>
	/// <remarks>
	/// We pass out MobManagers here so that the predicate function can decide whether
	/// or not to crack the shell if it wants to search inside.
	/// </remarks>
	public Mob findObject( Func<Mob, bool> predicate )
	{
		return findObjects( predicate ).FirstOrDefault();
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
			cur = findObject( 1 );	// ptb

		for( int i=1; i<components.Length; ++i )
		{
			if( components[i].StartsWithI("#") )
				throw new ArgumentException( "Path contains more than one absolute component" );
			cur = findObject( (m) =>
				cur.id == m.locationId &&
				components[i] == m.pathId
			);
			if( cur == null )
				return null;
		}

		return cur;
	}

	// These are just pass-throughs for the IWorld methods.
	public long ticks { get { return _world.ticks; } }

	public UrlGenerator attributeUrlGenerator
	{ get { return _world.attributeUrlGenerator; } }

	public WorldCheckpoint[] checkpoints { get { return _world.checkpoints; } }

	public void checkpoint( string name ) { _world.checkpoint( name ); }

	public void checkpointRemove( ulong id ) { _world.checkpointRemove( id ); }

	public Mob createObject() { return Mob.Wrap( _world.createObject() ); }

	public Mob findObject( int id ) { return Mob.Wrap( _world.findObject( id ) ); }

	public IEnumerable<Mob> findObjects( Func<Mob, bool> predicate )
	{
		// This is sort of wasteful but it simplifies a lot.
		return _world.findObjects( (imob) => predicate( Mob.Wrap( imob ) ) ).Select( m => Mob.Wrap( m ) ) ;
	}

	public void destroyObject( int id ) { _world.destroyObject( id ); }

	IWorld _world;
}

}
