namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptHost = Scripting.SSharp.SSharpScripting;

/// <summary>
/// The world: a collection of objects.
/// </summary>
public partial class World {
	// Only do the script init once.
	static World() {
		ScriptHost.Init();

		// These are all the types we allow the scripts direct access to, including objects passed down from the outside.
		// TODO: This needs to be in a config file somewhere.
		ScriptHost.AllowType(typeof(System.Object), "object");
		ScriptHost.AllowType(typeof(System.String));
		ScriptHost.AllowType(typeof(System.Text.StringBuilder));
		ScriptHost.AllowType(typeof(System.Math));
		ScriptHost.AllowType(typeof(System.Guid));
		ScriptHost.AllowType(typeof(System.DateTimeOffset), "DateTime");
		ScriptHost.AllowType(typeof(System.TimeSpan));
		ScriptHost.AllowType(typeof(System.Random));
		ScriptHost.AllowType(typeof(System.Uri));
		ScriptHost.AllowType(typeof(System.UriBuilder));
		ScriptHost.AllowType(typeof(System.UriComponents));
		ScriptHost.AllowType(typeof(System.UriFormat));
	}

	internal World() {
	}

	public delegate string UrlGenerator(Mob obj, string name);
	public UrlGenerator attributeUrlGenerator = null;

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
			newMob.attrSet(item.Name, item.Value);
		if (location.HasValue)
			newMob.locationId = location.Value;

		// Objects are parented onto the PTB by default. Otherwise...
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

	public Mob findObject(string path) {
		if (string.IsNullOrEmpty(path))
			return null;

		string[] components = path.Split(Mob.PathSep);
		Mob cur;
		if (components[0].StartsWith("#"))
			cur = findObject(CultureFree.ParseInt(components[0].Substring(1)));
		else
			cur = findObject(1);	// ptb

		for (int i=1; i<components.Length; ++i) {
			if (components[i].StartsWithI("#"))
				throw new ArgumentException("Path contains more than one absolute component");
			cur = findObject((m) =>
				cur.id == m.locationId &&
				components[i] == m.pathId);
			if (cur == null)
				return null;
		}

		return cur;
	}

	public Mob findObject(Func<Mob, bool> predicate) {
		foreach (var mob in _objects)
			if (predicate(mob.Value))
				return mob.Value;
		return null;
	}

	public IEnumerable<Mob> findObjects(Func<Mob, bool> predicate) {
		foreach (var mob in _objects)
			if (predicate(mob.Value))
				yield return mob.Value;
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
