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
/// Culture-insensitive parsing and such.
/// </summary>
static public class CultureFree {
	/// <summary>
	/// Quick access to the invariant culture for formatting and comparison.
	/// </summary>
	static public System.Globalization.CultureInfo Culture {
		get { return System.Globalization.CultureInfo.InvariantCulture; }
	}

	/// <summary>
	/// Culture-insensitive version of int.Parse().
	/// </summary>
	static public int ParseInt(string str) {
		return int.Parse(str, Culture);
	}

	/// <summary>
	/// Culture-insensitive string format.
	/// </summary>
	/// <param name="fmt">Format string</param>
	/// <param name="ps">Parameters for the format</param>
	/// <returns>The formatted string</returns>
	static public string Format( string fmt, params object[] ps )
	{
		return String.Format( Culture, fmt, ps );
	}
}

}
