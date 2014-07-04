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

namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Login {
	static public string LogUserIn(Session.UserContext cxt, string login, string password) {
		MooCore.Mob mob = null;
		using( var token = cxt.db.token() )
		{
			// FIXME: Hash password.
			var results = cxt.db.select(token, Models.User.Table, new Dictionary<string, object>() {
				{ "login", login },
				{ "password", password }
			});
			if (!results.Any())
				return "Invalid user name or password";

			Models.User u = Models.User.FromDatabase(results.First().Value);

			// We found a matching user record. Does the user have a Mob?
			if (u.objectid.HasValue) {
				mob = WorldData.world.findObject(u.objectid.Value);

				// If we fail here, there must be something funky. Fail out.
				if (mob == null)
					return "User has detached mob -- contact the admins, please.";
			} else {
				// Make a new Mob for the user.
				mob = WorldData.world.createObject(new {
						name = u.name
					},
					location: WorldData.world.findObject("/entry").id,
					parent: WorldData.world.findObject("/templates/player").id);

				// Save out their new Mob id to their account.
				u.objectid = mob.id;
				cxt.db.update(token, Models.User.Table, u.id, new Dictionary<string, object>() {
					{ "objectid", mob.id }
				});
			}
		}

		if (mob.player != null)
			cxt.player = mob.player;
		else
			cxt.player = new MooCore.Player(mob);

		return null;
	}

	static public void LogUserOut(Session.UserContext cxt) {
		cxt.player = null;
	}
}

}
