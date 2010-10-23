namespace Kayateia.Climoo.MooEngine {
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
		if (parent.HasValue)
			newMob.parentId = parent.Value;
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
