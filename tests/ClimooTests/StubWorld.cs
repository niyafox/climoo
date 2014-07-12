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

namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.MooCore;

// Basic mock of a World object.
class StubWorld : IWorld
{
	public void addObject( StubMob sm )
	{
		_mobs[sm.id] = sm;
	}
	Dictionary<int, StubMob> _mobs = new Dictionary<int,StubMob>();

	public long ticks { get { return 0; } }

	public World.UrlGenerator attributeUrlGenerator { get { return null; } }

	public WorldCheckpoint[] checkpoints { get { return null; } }

	public void checkpoint( string name ) { throw new NotImplementedException(); }

	public void checkpointRemove( ulong id ) { throw new NotImplementedException(); }

	public IMob createObject() { throw new NotImplementedException(); }

	public IMob findObject( int id )
	{
		if( _mobs.ContainsKey( id ) )
			return _mobs[id];
		else
			return null;
	}

	public IEnumerable<IMob> findObjects( Func<IMob, bool> predicate )
	{
		return _mobs.Values.Where( predicate );
	}

	public void destroyObject( int id )
	{
		_mobs.Remove( id );
	}
}

}
