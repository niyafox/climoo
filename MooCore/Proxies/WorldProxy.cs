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

namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// MOO Proxy object for the World object. This is available to MOO scripts.
/// </summary>
class WorldProxy : DynamicObjectBase {
	public WorldProxy(World w, Player p) {
		_w = w;
		_p = p;
	}

	World _w;
	Player _p;

	/// <summary>
	/// Finds an object in the world by ID and returns a proxy for it.
	/// </summary>
	/// <param name='id'>The object's ID.</param>
	[Passthrough]
	public MobProxy obj(int id) {
		Mob m = _w.findObject(id);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
	}

	/// <summary>
	/// Finds an object in the world by textual path and returns a proxy for it.
	/// </summary>
	/// <param name='path'>The object's path.</param>
	[Passthrough]
	public MobProxy obj(string path) {
		Mob m = _w.findObject(path);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
	}

	/// <summary>
	/// Creates a new object in the world and returns a proxy for it.
	/// </summary>
	[Passthrough]
	public MobProxy create() {
		Mob m = _w.createObject();
		m.parentId = 1;
		return new MobProxy(m, _p);
	}

	/// <summary>
	/// Deletes an object from the world by ID.
	/// </summary>
	/// <param name='id'>The ID to be deleted.</param>
	[Passthrough]
	public void del(int id) {
		_w.destroyObject(id);
	}

	/// <summary>
	/// Checkpoint the world database.
	/// </summary>
	[Passthrough]
	public void checkpoint() {
		_p.write("Checkpointing database...");
		_w.saveToSql();
		_p.write("Checkpoint finished.");
	}

	/// <summary>
	/// Returns a list of checkpoints in the world database.
	/// </summary>
	[Passthrough]
	public World.Checkpoint[] checkpoints {
		get { return _w.checkpoints; }
	}

	/// <summary>
	/// Removes a world database checkpoint by ID.
	/// </summary>
	/// <param name='id'>The checkpoint ID</param>
	[Passthrough]
	public void checkpointRemove(int id) {
		_p.write("Removing checkpoint {0}...".FormatI(id));
		_w.checkpointRemove(id);
		_p.write("Remove finished.");
	}
}

}
