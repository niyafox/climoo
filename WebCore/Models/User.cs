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
using System.Text;

/// <summary>
/// Represents a user for login.
/// </summary>
public class User {
	public const string Table = "user";

	public int id;
	public string login;
	public bool openid;
	public string password;
	public int? objectid;
	public string name;

	/// <summary>
	/// Loads a user from a database table row.
	/// </summary>
	static public User FromDatabase(IDictionary<string, object> values) {
		return new User() {
			id = (int)values["id"],
			login = (string)values["login"],
			openid = (bool)values["openid"],
			password = (string)values["password"],
			objectid = values["objectid"] == null ? null : (int?)values["objectid"],
			name = (string)values["name"]
		};
	}
}

}
