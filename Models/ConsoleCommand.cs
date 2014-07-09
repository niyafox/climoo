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

namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Represents a new command for the web console. If any of the items are blank/null, it
/// means there is no change / no output.
/// </summary>
public class ConsoleCommand {
	/// <summary>
	/// Text to be printed out to the user.
	/// </summary>
	public string text = "";

	/// <summary>
	/// A new prompt string.
	/// </summary>
	public string prompt = "";

	/// <summary>
	/// New sidebar contents.
	/// </summary>
	public string sidebar = "";

	/// <summary>
	/// Sound to be played.
	/// </summary>
	public string sound = "";
}

}
