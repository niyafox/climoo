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

namespace Kayateia.Climoo.MooCore.Exceptions
{
using System;

/// <summary>
/// Base class for all CliMOO-specific in-world exceptions. These will eventually
/// be passed down to scripts to give them a chance to handle errors.
/// </summary>
public class ClimooException : System.Exception
{
	public ClimooException() { }
	public ClimooException( string msg ) : base( msg ) { }
	public ClimooException( string msg, Exception inner ) : base( msg, inner ) { }
}

}
