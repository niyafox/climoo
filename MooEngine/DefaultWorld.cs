namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Define a default world if desired. This is kept away from the
// rest to keep the code cleaner.
public partial class World {
	static public World CreateDefault() {
		World w = new World();

		// This object will be the spiritual parent of every object in the MOO.
		Mob god = w.createObject(new {
			name = "Dawkins",
			desc = "The god object: It's all downhill from here, baby."
		});

		Mob templates = w.createObject(new {
			name = "Template Room",
			desc = "Everything that's a base object will go in here for easy findin's.",
			pathid = "templates"
		});

		Mob playerTemplate = w.createObject(new {
			name = "Player",
			desc = "A confused looking person wanders around with a 'help me' sticky note on their back.",
			pathid = "player"
		}, location: templates.id);

		Mob roomTemplate = w.createObject(new {
			name = "Room",
			desc = "A simple room, so simple really that it has no description.",
			pathid = "room"
		}, location: templates.id);

		return w;
	}
}

}
