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

		// This object will be the spiritual parent of every object in the MOO. Its
		// ID should always be #1.
		Mob ptb = w.createObject(new {
			name = "The Powers That Be",
			desc = "The 'god' object: It's all downhill from here, baby."
		}, parent: Mob.None.id);
		System.Diagnostics.Debug.Assert(ptb.id == 1);

		Mob templates = w.createObject(new {
			name = "Template Room",
			desc = "Everything that's a base object will go in here for easy findin's.",
			pathid = "templates"
		}, location: ptb.id);

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
		roomTemplate.verbs["look"] = new Verb() {
			name = "look",
			help = "Look at this object",
			code = @"
				sb = new StringBuilder();
				sb.AppendFormat(""[b]{0}[/b] ({1})"",
					self.name,
					self.fqpn);
				if (self.image)
					sb.AppendFormat(""[float=right]{0}[/float]"", self.image);
				sb.AppendFormat(""\n{0}\n"", self.desc);
				player.write(sb.ToString());
			"
		};

		Mob entryWay = w.createObject(new {
			name = "The White Room",
			desc = "You're standing in a nearly featureless room, and everything is white white white. "
				+ "It looks like a long hallway, about 10 paces across, and with metal ceiling structures "
				+ "overhead. In the other directions, it seems to go on forever.",
			pathid = "entry",
			image = TypedAttribute.LoadFromFile(@"d:\game\climoo\gamedata\whiteroom.jpg")
		}, location: ptb.id, parent: roomTemplate.id);

		Mob conduit = w.createObject(new {
			name = "The Conduit",
			desc = "A little girl in a red dress, or a panther with the coat of deepest black; it really "
				+ "depends on who's looking.",
			pathid = "conduit" /*,
			image = TypedAttribute.LoadFromFile(@"d:\game\climoo\gamedata\conduit.jpg") */
		}, location: entryWay.id);

		return w;
	}
}

}
