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
		public const string Id = "id";
		public const string Parent = "parent";
		public const string Location = "loc";
		public const string PathId = "pathid";

		public const string Name = "name";
		public const string Description = "desc";
		public const string Image = "image";		// Should be an image blob
	}

	public const char PathSep = ':';

	/// <summary>
	/// Object's local ID, a single int like an inode number. This is auto-generated, and is used
	/// for all real internal references.
	/// </summary>
	public int id {
		get {
			return _id;
		}
		private set {
			_id = value;
			_attributes[Attributes.Id] = _id;
		}
	}

	/// <summary>
	/// Object's parent ID. This is used for object inheritance, for properties and verbs.
	/// </summary>
	public int parentId {
		get { return _parentId; }
		set {
			_parentId = value;
			_attributes[Attributes.Parent] = _parentId;
		}
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
		set {
			_locationId = value;
			_attributes[Attributes.Location] = _locationId;
		}
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
		get { return (string)findAttribute(Attributes.Name, true); }
		set { _attributes[Attributes.Name] = value; }
	}
	public string desc {
		get { return (string)findAttribute(Attributes.Description, true); }
		set { _attributes[Attributes.Description] = value; }
	}
	public string pathId {
		get { return (string)findAttribute(Attributes.PathId, true); }
		set { _attributes[Attributes.PathId] = value; }
	}

	/// <summary>
	/// Access to the object's local verbs, for add, remove, and enumerate.
	/// </summary>
	public IDictionary<StringI, Verb> verbs {
		get { return _verbs; }
	}

	/// <summary>
	/// Access to the object's local attributes, for add, remove, and enumerate.
	/// </summary>
	/// <remarks>Everything in this should be a String or TypedAttribute.</remarks>
	public IDictionary<StringI, object> attributes {
		get { return _attributes; }
	}

	/// <summary>
	/// Returns a read-only collection of all the available verbs to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, Verb> allVerbs {
		get {
			var recursiveDict = new Dictionary<StringI,Verb>();
			getAllVerbs(recursiveDict);
			return recursiveDict;
		}
	}

	void getAllVerbs(IDictionary<StringI, Verb> targetList) {
		// Get the full parent chain list.
		if (this.parentId > 0) {
			var parentObj = this.parent;
			if (parentObj != null)
				parentObj.getAllVerbs(targetList);
		}

		// Replace any in the list with local ones.
		foreach (var item in this.verbs)
			targetList[item.Key] = item.Value;
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
	/// <returns>An object with the attribute's contents, or null if not found.</returns>
	public object findAttribute(string name, bool localOnly = false) {
		return traverseInheritance((obj) => {
			if (obj._attributes.ContainsKey(name))
				return obj._attributes[name];
			else
				return null;
		});
	}

	/// <summary>
	/// Same as findAttribute(), but guarantees a TypedAttribute in return. Plain
	/// string attributes will be wrapped with a text/plain type, and MOO objects
	/// will be wrapped with moo/object.
	/// </summary>
	/// <param name="name">The attribute name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A TypedAttribute with the attribute's contents, or null if not found.</returns>
	public TypedAttribute findAttributeAndType(string name, bool localOnly = false) {
		object rv = findAttribute(name, localOnly);
		if (rv is TypedAttribute)
			return rv as TypedAttribute;
		else
			return new TypedAttribute() { contents = rv, mimetype = "text/plain" };
	}

	/// <summary>
	/// Returns a fully qualified path name, which may be used in a more human-friendly
	/// manner to find certain named objects within a hierarchy of objects.
	/// </summary>
	public string fqpn {
		get {
			// Find my path component.
			string me;
			if (!_attributes.ContainsKey(Attributes.PathId) || _locationId <= 0) {
				if (_id == 1)
					me = "";
				else
					me = StringCase.FormatI("#{0}", _id);
			} else {
				// Put my path name on the back.
				me = _attributes[Attributes.PathId].ToString();

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
	Dictionary<StringI, object> _attributes = new Dictionary<StringI,object>();
}

}
