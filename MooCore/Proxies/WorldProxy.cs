namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

class WorldProxy : DynamicObjectBase {
	public WorldProxy(World w, Player p) {
		_w = w;
		_p = p;
	}

	World _w;
	Player _p;

	[Passthrough]
	public MobProxy obj(int id) {
		Mob m = _w.findObject(id);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
	}

	[Passthrough]
	public MobProxy obj(string path) {
		Mob m = _w.findObject(path);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
	}

	[Passthrough]
	public void del(int id) {
		_w.destroyObject(id);
	}

	[Passthrough]
	public void checkpoint() {
		_p.write("Checkpointing database...");
		_w.saveToSql();
		_p.write("Checkpoint finished.");
	}

	[Passthrough]
	public World.Checkpoint[] checkpoints {
		get { return _w.checkpoints; }
	}

	[Passthrough]
	public void checkpointRemove(int id) {
		_p.write("Removing checkpoint {0}...".FormatI(id));
		_w.checkpointRemove(id);
		_p.write("Remove finished.");
	}
}

}
