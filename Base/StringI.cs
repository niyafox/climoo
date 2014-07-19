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

namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Thin wrapper for String that provides case and culture
/// insensitive comparison operators.
/// </summary>
public class StringI {
	public StringI(string s) { _s = s; }

	public override int GetHashCode() {
		return _s.ToLowerInvariant().GetHashCode();
	}

	public override string ToString() {
		return _s;
	}

	public override bool Equals(object obj) {
		if (!(obj is StringI) && !(obj is string))
			return false;

		string b;
		if (obj is StringI)
			b = ((StringI)obj)._s;
		else
			b = obj as string;

		return _s.EqualsI(b);
	}

	static public bool operator==(StringI a, StringI b) {
		if( System.Object.ReferenceEquals( a, b ) )
			return true;

		if( ((object)a == null && (object)b != null) || ((object)a != null && (object)b == null) )
			return false;
		else
			return a._s.EqualsI(b._s);
	}

	static public bool operator!=(StringI a, StringI b) {
		return !(a == b);
	}

	static public StringI operator+(StringI a, StringI b) {
		return new StringI(a._s + b._s);
	}

	static public implicit operator StringI(string si) {
		return new StringI(si);
	}

	static public implicit operator string(StringI si) {
		return si._s;
	}

	string _s;
}

}
