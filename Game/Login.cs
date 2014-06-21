namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Login {
	static public string LogUserIn(Session.UserContext cxt, string login, string password) {
		// FIXME: Hash password.
		var results = cxt.db.select(Models.User.Table, new Dictionary<string, object>() {
			{ "login", login }
		});
		if (!results.Any())
			return "Invalid user name or password";

		Models.User u = Models.User.FromDatabase(results.First().Value);

		// We found a matching user record. Does the user have a Mob?
		MooCore.Mob mob = null;
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
			cxt.db.update(Models.User.Table, u.id, new Dictionary<string, object>() {
				{ "objectid", mob.id }
			});
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
