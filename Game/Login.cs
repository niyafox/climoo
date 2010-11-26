namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Login {
	static public string LogUserIn(Session.UserContext cxt, string login, string password) {
		using (var context = new Models.ClimooDataContext()) {
			context.Connection.Open();

			// FIXME: Hash password.
			var usertable = context.GetTable<Models.User>();
			var record = (from x in usertable
				where x.login == login && x.password == password
				select x).FirstOrDefault();

			if (record == null)
				return "Invalid user name or password";

			// We found a matching user record. Does the user have a Mob?
			MooCore.Mob mob = null;
			if (record.objectid.HasValue) {
				mob = WorldData.world.findObject(record.objectid.Value);

				// If we fail here, there must be something funky. Fail out.
				if (mob == null)
					return "User has detached mob -- contact the admins, please.";
			} else {
				// Make a new Mob for the user.
				mob = WorldData.world.createObject(new {
						name = record.name
					},
					location: WorldData.world.findObject("/entry").id,
					parent: WorldData.world.findObject("/templates/player").id);

				// Save out their new Mob id to their account.
				record.objectid = mob.id;
				context.SubmitChanges();
			}

			if (mob.player != null)
				cxt.player = mob.player;
			else
				cxt.player = new MooCore.Player(mob);

			return null;
		}
	}

	static public void LogUserOut(Session.UserContext cxt) {
		cxt.player = null;
	}
}

}
