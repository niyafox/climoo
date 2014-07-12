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
/// Pretty much what it says -- a contained object with a timestamp.
/// </summary>
public class Timestamped<T>
{
	public Timestamped( T obj, DateTimeOffset stamp )
	{
		_obj = obj;
		_stamp = stamp;
	}

	public Timestamped( T obj )
	{
		_obj = obj;
		_stamp = DateTimeOffset.UtcNow;
	}

	public T get
	{
		get { return _obj; }
	}

	public DateTimeOffset stamp
	{
		get { return _stamp; }
	}

	// For the purposes of hashing and equality comparison, we don't worry about
	// the timestamp. This lets us throw the objects in collections and still work
	// with them intelligently.
	public override int GetHashCode()
	{
		return _obj.GetHashCode();
	}

	public override string ToString()
	{
		return _obj.ToString();
	}

	public override bool Equals( object obj )
	{
		if( obj is Timestamped<T> )
			return _obj.Equals( ((Timestamped<T>)obj)._obj );
		else
			return _obj.Equals( obj );
	}

	T _obj;
	DateTimeOffset _stamp;
}

}
