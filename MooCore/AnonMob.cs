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
/// Represents an anonymous mob, or a mob that is not really there, but represents
/// an un-logged-in player. These are like ghosts that all have the same ID and don't
/// truly exist in the world with the rest of the mobs.
/// </summary>
public class AnonMob : IMob
{
	public AnonMob( IWorld world, Player player )
	{
		_world = world;
		_player = player;
	}

	public int id { get { return Mob.Anon.id; } }

	public int parentId
	{
		get { return 0; }
		set { }
	}

	public int locationId
	{
		get
		{
			if( !_locationId.HasValue )
			{
				World w = World.Wrap( _world );
				Mob fp = w.findObject( World.WellKnownObjects.FrontPorch );
				if( fp == null )
					throw new InvalidOperationException( "There's no front porch." );
				_locationId = fp.id;
			}
			return _locationId.Value;
		}
		set { _locationId = value; }
	}
	int? _locationId;

	public int ownerId
	{
		get { return 0; }
		set { }
	}

	public void verbSet( StringI name, Verb v ) { }
	public Verb verbGet( StringI name ) { return null; }
	public void verbDel( StringI name ) { }

	public IEnumerable<StringI> verbList
	{
		get { return new StringI[0]; }
	}

	public void attrSet( StringI name, TypedAttribute v ) { }
	public TypedAttribute attrGet( StringI name ) { return null; }
	public void attrDel( StringI name ) { }

	public IEnumerable<StringI> attrList
	{
		get { return new StringI[0]; }
	}

	public Player player
	{
		get { return _player; }
		set { _player = value; }
	}
	Player _player;

	public IWorld world
	{
		get { return _world; }
	}
	IWorld _world;
}

}
