namespace Kayateia.Climoo.MooEngine {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruby = Kayateia.Climoo.Scripting.Ruby;

/// <summary>
/// MOO OBject -- represents any world object within the MOO.
/// </summary>
// FIXME: Needs locking
public class Mob {
	internal Mob(World world, int id) {
		_world = world;
		this.id = id;
	}

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
	}

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

	/// <summary>
	/// Access to the object's local verbs, for add, remove, and enumerate.
	/// </summary>
	public IDictionary<string, Verb> verbs {
		get { return _verbs; }
	}

	/// <summary>
	/// Access to the object's local attributes, for add, remove, and enumerate.
	/// </summary>
	public IDictionary<string, object> attributes {
		get { return _attributes; }
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
	/// <returns>A Verb object for the verb, or null if not found.</returns>
	public Verb findVerb(string verb) {
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
	/// <returns>An object with the attribute's contents, or null if not found.</returns>
	public object findAttribute(string name) {
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
			if (!_attributes.ContainsKey(Attributes.PathId))
				me = string.Format("#{0}", _id);
			else
				me = _attributes[Attributes.PathId].ToString();

			// Add the parent path, if we have one.
			if (_parentId > 0) {
				Mob parentMob = _world.findObject(_parentId);
				if (parentMob != null)
					me = string.Format("{0}:{1}", parentMob.fqpn, me);
				else {
					// FIXME: Invalid parent. Log or something.
					return string.Format("#{0}", _id);
				}
			}

			return me;
		}
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
	Dictionary<string, Verb> _verbs = new Dictionary<string, Verb>();

	// Attributes on the object
	Dictionary<string, object> _attributes = new Dictionary<string,object>();
}

}
