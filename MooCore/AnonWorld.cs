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
/// Represents an anonymous world, which is a wrapper around a regular world. This
/// removes all writing operations from the world itself, and passes through everything else.
/// </summary>
public class AnonWorld : IWorld
{
	public long ticks
	{
		get { return _real.ticks; }
	}

	public World.UrlGenerator attributeUrlGenerator
	{
		get { return _real.attributeUrlGenerator; }
	}

	public WorldCheckpoint[] checkpoints
	{
		get { return new WorldCheckpoint[0]; }
	}

	public void checkpoint( string name ) { }

	public void checkpointRemove( ulong id ) { }

	public IMob createObject()
	{
		return null;
	}

	public IMob findObject( int id )
	{
		if( id == Mob.Anon.id )
			return _anonMob;
		else
			return _real.findObject( id );
	}

	public IEnumerable<IMob> findObjects( Func<IMob, bool> predicate )
	{
		/* if( predicate( _anonMob ) )
			yield return _anonMob; */

		foreach( var m in _real.findObjects( predicate ) )
			yield return m;
	}

	public void destroyObject( int id ) { }

	public IWorld real
	{
		get { return _real; }
		set { _real = value; }
	}
	IWorld _real;

	public IMob anonMob
	{
		get { return _anonMob; }
		set { _anonMob = value; }
	}
	IMob _anonMob;
}

}
