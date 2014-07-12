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
/// Essentually a mock of IMob that just acts as a placeholder and has fixed IDs.
/// </summary>
public class SpecialMob : IMob
{
	public SpecialMob( int id, int parentId )
	{
		this.id = id;
		this.parentId = parentId;
	}
	
	public int id
	{
		get; private set;
	}

	public int parentId
	{
		get; set;
	}

	public int locationId
	{
		get
		{
			return 1;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int ownerId
	{
		get
		{
			return 1;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public Perm perms
	{
		get
		{
			return 0;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public void verbSet( StringI name, Verb v )
	{
		throw new NotImplementedException();
	}

	public Verb verbGet( StringI name )
	{
		return null;
	}

	public void verbDel( StringI name )
	{
		throw new NotImplementedException();
	}

	public IEnumerable<StringI> verbList
	{
		get
		{
			return new StringI[0];
		}
	}

	public bool attrHas( StringI name )
	{
		return false;
	}

	public void attrSet( StringI name, TypedAttribute v )
	{
		throw new NotImplementedException();
	}

	public TypedAttribute attrGet( StringI name )
	{
		return null;
	}

	public void attrDel( StringI name )
	{
		throw new NotImplementedException();
	}

	public IEnumerable<StringI> attrList
	{
		get
		{
			return new StringI[0];
		}
	}

	public Player player
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public IWorld world
	{
		get
		{
			return null;
		}
	}
}

}
