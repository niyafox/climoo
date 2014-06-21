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

using Kayateia.Climoo.Database;

/// <summary>
/// Handles stuffing models data from an XML export into an existing database.
/// This can be done once for a proper database backing, or at load time for
/// memory database usage.
/// </summary>
public class XmlModelPersistence {
	static public void Import(string sourceFn, IDatabase target) {
		XmlClimooWeb web = XmlPersistence.Load<XmlClimooWeb>(sourceFn);

		foreach (var s in web.screens) {
			target.insert(Screen.Table, new Dictionary<string, object>() {
				{ "name", s.name },
				{ "text", s.text }
			});
		}

		foreach (var u in web.users) {
			target.insert(User.Table, new Dictionary<string, object>() {
				{ "login", u.login },
				{ "openid", u.openId },
				{ "password", u.password },
				{ "objectid", u.objectId == 0 ? null : (int?)u.objectId },
				{ "name", u.name }
			});
		}
	}
}

}
