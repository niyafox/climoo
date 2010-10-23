namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The world: a collection of objects.
/// </summary>
public partial class World {
	public World() {
	}

	public Mob createObject() {
		lock (_mutex) {
			int id = ++_nextId;
			Mob newMob = new Mob(this, id);
			_objects[id] = newMob;
			return newMob;
		}
	}

	public Mob createObject(object attributes, int? location = null, int? parent = null) {
		Mob newMob = createObject();
		foreach (var item in PropertyEnumerator.GetProperties(attributes))
			newMob.attributes[item.Name] = item.Value;
		if (location.HasValue)
			newMob.locationId = location.Value;

		// Objects are parented onto the PTB by default.
		if (parent.HasValue) {
			if (parent.Value != -1)
				newMob.parentId = parent.Value;
		} else
			newMob.parentId = 1;

		return newMob;
	}

	public Mob findObject(int id) {
		lock (_mutex) {
			if (_objects.ContainsKey(id))
				return _objects[id];
			else
				return null;
		}
	}

	public void destroyObject(int id) {
		// Do we ever want to reclaim IDs?
		lock (_mutex) {
			_objects.Remove(id);
		}
	}

	object _mutex = new object();
	int _nextId = 0;
	Dictionary<int, Mob> _objects = new Dictionary<int,Mob>();
}

}
