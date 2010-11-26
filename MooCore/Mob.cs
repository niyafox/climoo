namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// MOO OBject -- represents any world object within the MOO.
/// </summary>
// FIXME: Needs locking
public class Mob {
	internal Mob(World world, int id) {
		_world = world;
		this.id = id;
	}

	private Mob() { }

	static public Mob Ambiguous {
		get { return s_ambig; }
	}
	static Mob s_ambig = new Mob() {
		id = -2,
		parentId = -3
	};

	static public Mob None {
		get { return s_none; }
	}
	static Mob s_none = new Mob() {
		id = -3,
		parentId = -3
	};

	/// <summary>
	/// Well-known attribute IDs
	/// </summary>
	public static class Attributes {
		public const string PathId = "pathid";
		public const string Name = "name";
		public const string Description = "desc";
		public const string Image = "image";		// Should be an image blob
	}

	public const char PathSep = '/';

	/// <summary>
	/// Mob reference -- weak reference good for attributes and persistence.
	/// </summary>
	public class Ref {
		public Ref(Mob m) {
			_id = m.id;
		}

		public Ref(int id) {
			_id = id;
		}

		public int id { get { return _id; } }

		readonly int _id;
	}

	/// <summary>
	/// Object's local ID, a single int like an inode number. This is auto-generated, and is used
	/// for all real internal references.
	/// </summary>
	public int id {
		get { return _id; }
		private set { _id = value; }
	}

	/// <summary>
	/// Object's parent ID. This is used for object inheritance, for properties and verbs.
	/// </summary>
	public int parentId {
		get { return _parentId; }
		set { _parentId = value; }
	}
	public Mob parent {
		get {
			if (this.parentId >= 0)
				return _world.findObject(this.parentId);
			else
				return null;
		}
	}

	/// <summary>
	/// Object's location ID. This is where the object is located, and is used for
	/// looking things up by path.
	/// </summary>
	public int locationId {
		get { return _locationId; }
		set { _locationId = value; }
	}
	public Mob location {
		get {
			if (this.locationId >= 0)
				return _world.findObject(this.locationId);
			else
				return null;
		}
	}

	// Convenience get/set for a few common attributes.
	public string name {
		get { return NullOrStr(findAttribute(Attributes.Name, true)); }
		set { _attributes[Attributes.Name] = TypedAttribute.FromValue(value); }
	}
	public string desc {
		get { return NullOrStr(findAttribute(Attributes.Description, true)); }
		set { _attributes[Attributes.Description] = TypedAttribute.FromValue(value); }
	}
	public string pathId {
		get { return NullOrStr(findAttribute(Attributes.PathId, true)); }
		set { _attributes[Attributes.PathId] = TypedAttribute.FromValue(value); }
	}

	static string NullOrStr(TypedAttribute attr) {
		if (attr == null)
			return null;
		else
			return attr.str;
	}

	/// <summary>
	/// Access to the object's local verbs, for add, remove, and enumerate.
	/// </summary>
	public bool verbHas(StringI name) { return _verbs.ContainsKey(name); }
	public void verbSet(StringI name, Verb v) { _verbs[name] = v; }
	public Verb verbGet(StringI name) {
		if (verbHas(name))
			return _verbs[name];
		else
			return null;
	}
	public void verbDel(StringI name) { _verbs.Remove(name); }
	public IEnumerable<string> verbList {
		get { return from k in _verbs.Keys select (string)k; }
	}

	/// <summary>
	/// Access to the object's local attributes, for add, remove, and enumerate.
	/// </summary>
	public bool attrHas(StringI name) { return _attributes.ContainsKey(name); }
	public void attrSet(StringI name, object v) { _attributes[name] = TypedAttribute.FromValue(v); }
	public TypedAttribute attrGet(StringI name) {
		if (attrHas(name))
			return _attributes[name];
		else
			return null;
	}
	public void attrDel(StringI name) { _attributes.Remove(name); }
	public IEnumerable<string> attrList {
		get { return from k in _attributes.Keys select (string)k; }
	}

	/// <summary>
	/// Returns a read-only collection of all the available attributes to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<TypedAttribute>> allAttrs {
		get {
			var list = new Dictionary<StringI, SourcedItem<TypedAttribute>>();
			traverseInheritance(null, (mob) => {
				// Replace any in the list with local ones.
				foreach (var item in _attributes)
					list[item.Key] = new SourcedItem<TypedAttribute>(this, item.Key, item.Value);
			});

			return list;
		}
	}

	/// <summary>
	/// Returns a read-only collection of all the available verbs to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<Verb>> allVerbs {
		get {
			var list = new Dictionary<StringI,SourcedItem<Verb>>();
			traverseInheritance(null, (mob) => {
				// Replace any in the list with local ones.
				foreach (var item in mob._verbs)
					list[item.Key] = new SourcedItem<Verb>(mob, item.Key, item.Value);
			});
			return list;
		}
	}

	// Generic tree traversal for looking for something through inheritance.
	T traverseInheritance<T>(Func<Mob, T> finder) where T : class {
		var rv = finder(this);
		if (rv != null)
			return rv;
		else {
			if (_parentId > 0) {
				Mob parentMob = _world.findObject(_parentId);
				if (parentMob != null)
					return parentMob.traverseInheritance(finder);
				else {
					// FIXME: Invalid parent. Log or something.
					return null;
				}
			} else
				return null;
		}
	}

	// Generic tree traversal, non-search.
	void traverseInheritance(Action<Mob> visitorPre, Action<Mob> visitorPost) {
		if (visitorPre != null)
			visitorPre(this);

		if (_parentId > 0) {
			Mob parentMob = _world.findObject(_parentId);
			if (parentMob != null)
				parentMob.traverseInheritance(visitorPre, visitorPost);
		}

		if (visitorPost != null)
			visitorPost(this);
	}

	/// <summary>
	/// Looks for a named verb on us, or on a parent object.
	/// </summary>
	/// <param name="verb">The verb name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A Verb object for the verb, or null if not found.</returns>
	public Verb findVerb(string verb, bool localOnly = false) {
		return traverseInheritance((obj) => {
			if (obj._verbs.ContainsKey(verb))
				return obj._verbs[verb];
			else
				return null;
		});
	}

	/// <summary>
	/// Looks for a named attribute on us, or on a parent object.
	/// </summary>
	/// <param name="name">The attribute name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A TypedAttribute with the attribute's contents, or null if not found.</returns>
	public TypedAttribute findAttribute(string name, bool localOnly = false) {
		return traverseInheritance((obj) => {
			if (obj._attributes.ContainsKey(name))
				return obj._attributes[name];
			else
				return null;
		});
	}

	/// <summary>
	/// Returns a fully qualified path name, which may be used in a more human-friendly
	/// manner to find certain named objects within a hierarchy of objects.
	/// </summary>
	public string fqpn {
		get {
			// Find my path component.
			string me;
			if (!attrHas(Attributes.PathId) || attrGet(Attributes.PathId).str.IsNullOrEmpty() || _locationId <= 0) {
				if (_id == 1)
					me = "";
				else
					me = StringCase.FormatI("#{0}", _id);
			} else {
				// Put my path name on the back.
				me = attrGet(Attributes.PathId).str;

				// Add our location's path.
				Mob locMob = _world.findObject(_locationId);
				if (locMob != null)
					me = StringCase.FormatI("{0}{1}{2}", locMob.fqpn, PathSep, me);
				else {
					// FIXME: Invalid parent. Log or something.
					me = StringCase.FormatI("#{0}", _id);
				}
			}

			return me;
		}
	}

	public IEnumerable<Mob> contained {
		get {
			return _world.findObjects((m) => m.locationId == _id);
		}
	}

	public World world { get { return _world; } }

	public bool isDescendentOf(int id) {
		return traverseInheritance((m) => m.id == id ? "" : null) != null;
	}

	/// <summary>
	/// The player associated with this Mob (i.e. this Mob is its avatar).
	/// </summary>
	/// <remarks>
	/// These are pretty ephemeral -- they can be associated when the user logs in and
	/// may not be valid later. Additionally they are not stored in to the DB in anyway.
	/// </remarks>
	public Player player { get; set; }


	// The reality we belong to.
	World _world;

	// IDs
	int _id;

	// Parent ID (local only)
	int _parentId;

	// Location ID (local only)
	int _locationId;

	// Verbs attached to the object
	Dictionary<StringI, Verb> _verbs = new Dictionary<StringI, Verb>();

	// Attributes on the object
	Dictionary<StringI, TypedAttribute> _attributes = new Dictionary<StringI,TypedAttribute>();
}

}
