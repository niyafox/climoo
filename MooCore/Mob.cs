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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// MOO OBject -- represents any world object within the MOO.
/// </summary>
/// <remarks>
/// It's very important that all changes to Mobs go through this class itself. Things
/// that pull SourcedItems are okay because those notify Mob that they changed, but
/// otherwise things like receiving Verbs from here should be considered read-only.
/// </remarks>
public class Mob {
	internal Mob(World world, int id) {
		_lock = new object();
		_world = world;
		_id = id;
		_parentId = 1;
		_ownerId = None.id;
		_perms = Perm.R | Perm.F;
	}

	private Mob( int id, int parentId )
	{
		_lock = new object();
		_id = id;
		_parentId = parentId;
	}

	/// <summary>
	/// Returns the Ambiguous object, used in verb matching.
	/// </summary>
	static public Mob Ambiguous {
		get { return s_ambig; }
	}
	static Mob s_ambig = new Mob( id: -2, parentId: -3 );

	/// <summary>
	/// Returns the None object, used in verb matching.
	/// </summary>
	static public Mob None {
		get { return s_none; }
	}
	static Mob s_none = new Mob( id: -3, parentId: -3 );

	/// <summary>
	/// Well-known attribute IDs
	/// </summary>
	public static class Attributes {
		public const string PathId = "pathid";
		public const string Name = "name";
		public const string Description = "desc";
		public const string Image = "image";		// Should be an image blob
		public const string Owner = "owner";		// Should be a mobref
		public const string Permissions = "perms";	// Int bitfield
		public const string PulseVerb = "pulseverb";
		public const string PulseFrequency = "pulsefreq";	// Should be an int
	}

	/// <summary>
	/// Path separator for addressing objects by path.
	/// </summary>
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
		get
		{
			lock( _lock )
				return _id;
		}
		private set
		{
			lock( _lock )
			{
				_id = value;
				changed();
			}
		}
	}

	/// <summary>
	/// Object's parent ID. This is used for object inheritance, for properties and verbs.
	/// </summary>
	public int parentId {
		get
		{
			lock( _lock )
				return _parentId;
		}
		set
		{
			lock( _lock )
			{
				_parentId = value;
				changed();
			}
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
		get {
			lock( _lock )
				return _locationId;
		}
		set
		{
			lock( _lock )
			{
				_locationId = value;
				changed();
			}
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

	/// <summary>
	/// ID of the owner of this object.
	/// </summary>
	public int ownerId {
		get 
		{
			lock( _lock )
				return _ownerId;
		}
		set
		{
			lock( _lock )
			{
				_ownerId = value;
				changed();
			}
		}
	}
	public Mob owner {
		get {
			if (this.ownerId >= 0)
				return _world.findObject(this.ownerId);
			else
				return null;
		}
	}

	/// <summary>
	/// Object's access permissions.
	/// </summary>
	/// <remarks>Default is R+F.</remarks>
	public Perm perms {
		get {
			lock( _lock )
				return _perms;
		}
		set {
			lock( _lock )
			{
				if (value & ~(Perm.R | Perm.W | Perm.F | Perm.Coder | Perm.Mayor | Perm.Player))
					throw new InvalidOperationException("Only R, W, F, Coder, Mayor, and Player permissions are valid for mobs");
				_perms = value;
				changed();
			}
		}
	}

	// These handle database saving management for mobs.
	public void changed()
	{
		lock( _lock )
			_changed = true;
		// _world.objectChanged( this );
	}
	public bool hasChanged()
	{
		lock( _lock )
			return _changed;
	}
	public void resetChanged()
	{
		lock( _lock )
			_changed = false;
	}

	// TODO: When a new attr or verb is set over the old one, copy the perms

	// Convenience get/set for a few common attributes.
	public string name {
		get { return NullOrStr(findAttribute(Attributes.Name, true)); }
		set
		{
			attrSet(Attributes.Name, TypedAttribute.FromValue(value));
			changed();
		}
	}
	public string desc {
		get { return NullOrStr(findAttribute(Attributes.Description, true)); }
		set
		{
			attrSet(Attributes.Description, TypedAttribute.FromValue(value));
			changed();
		}
	}
	public string pathId {
		get { return NullOrStr(findAttribute(Attributes.PathId, true)); }
		set
		{
			attrSet(Attributes.PathId, TypedAttribute.FromValue(value));
			changed();
		}
	}
	public string pulseVerb
	{
		get
		{
			return NullOrStr( findAttribute( Attributes.PulseVerb, true ) );
		}
	}
	public int pulseFreq
	{
		get
		{
			return NullOrZero( findAttribute( Attributes.PulseFrequency, true ) );
		}
	}

	static int NullOrZero(TypedAttribute attr) {
		if (attr == null)
			return 0;
		else
			return attr.getContents<int>();
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
	public bool verbHas(StringI name)
	{
		lock( _lock )
			return _verbs.ContainsKey(name);
	}
	public void verbSet(StringI name, Verb v) {
		lock( _lock )
		{
			// If there's an old verb, copy over permissions and make
			// it appear that we just replaced the contained value.
			if (_verbs.ContainsKey(name))
				v.perms = _verbs[name].perms;

			_verbs[name] = v;
			changed();
		}
	}
	public Verb verbGet(StringI name) {
		lock( _lock )
		{
			if (verbHas(name))
				return _verbs[name];
			else
				return null;
		}
	}
	public void verbDel(StringI name)
	{
		lock( _lock )
		{
			_verbs.Remove(name);
			changed();
		}
	}
	public IEnumerable<string> verbList {
		get
		{
			// Use ToArray here to avoid lock slicing.
			lock( _lock )
				return (from k in _verbs.Keys select (string)k).ToArray();
		}
	}

	/// <summary>
	/// Access to the object's local attributes, for add, remove, and enumerate.
	/// </summary>
	public bool attrHas(StringI name) {
		lock( _lock )
			return _attributes.ContainsKey(name);
	}
	public void attrSet(StringI name, object v) {
		lock( _lock )
		{
			TypedAttribute newattr = TypedAttribute.FromValue(v);

			// If there's an old attribute, copy over permissions and make
			// it appear that we just replaced the contained value.
			if (_attributes.ContainsKey(name))
				newattr.perms = _attributes[name].perms;

			_attributes[name] = newattr;

			changed();
			if( name == Attributes.PulseVerb || name == Attributes.PulseFrequency )
				_world.pulseCheck( this );
		}
	}
	public TypedAttribute attrGet(StringI name) {
		lock( _lock )
		{
			if (attrHas(name))
				return _attributes[name];
			else
				return null;
		}
	}
	public void attrDel(StringI name)
	{
		lock( _lock )
			_attributes.Remove(name);
	}
	public IEnumerable<string> attrList {
		get
		{
			// Use ToArray here to avoid lock slicing.
			lock( _lock )
				return (from k in _attributes.Keys select (string)k).ToArray();
		}
	}

	/// <summary>
	/// Returns a read-only collection of all the available attributes to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<TypedAttribute>> allAttrs {
		get {
			lock( _lock )
			{
				var list = new Dictionary<StringI, SourcedItem<TypedAttribute>>();
				traverseInheritance(null, (mob) => {
					// Replace any in the list with local ones.
					foreach (var item in _attributes)
						list[item.Key] = new SourcedItem<TypedAttribute>(this, item.Key, item.Value);
				});

				return list;
			}
		}
	}

	/// <summary>
	/// Returns a read-only collection of all the available verbs to this object,
	/// including things inherited from parents.
	/// </summary>
	public IDictionary<StringI, SourcedItem<Verb>> allVerbs {
		get {
			lock( _lock )
			{
				var list = new Dictionary<StringI,SourcedItem<Verb>>();
				traverseInheritance(null, (mob) => {
					// Replace any in the list with local ones.
					foreach (var item in mob._verbs)
						list[item.Key] = new SourcedItem<Verb>(mob, item.Key, item.Value);
				});
				return list;
			}
		}
	}

	// Generic tree traversal for looking for something through inheritance.
	T traverseInheritance<T>(Func<Mob, T> finder) where T : class {
		var rv = finder(this);
		if (rv != null)
			return rv;
		else {
			if (_parentId > 0) {
				if (_parentId == _id)
					throw new InvalidOperationException("Mob is its own parent; call out the time squad");
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
			if (_parentId == _id)
				throw new InvalidOperationException("Mob is its own parent; call out the time squad");
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
		lock( _lock )
		{
			return traverseInheritance((obj) => {
				if (obj._verbs.ContainsKey(verb))
					return obj._verbs[verb];
				else
					return null;
			});
		}
	}

	/// <summary>
	/// Looks for a named attribute on us, or on a parent object.
	/// </summary>
	/// <param name="name">The attribute name</param>
	/// <param name="localOnly">False (default) if we're to search the inheritance hierarchy</param>
	/// <returns>A TypedAttribute with the attribute's contents, or null if not found.</returns>
	public TypedAttribute findAttribute(string name, bool localOnly = false) {
		lock( _lock )
		{
			return traverseInheritance((obj) => {
				if (obj._attributes.ContainsKey(name))
					return obj._attributes[name];
				else
					return null;
			});
		}
	}

	/// <summary>
	/// Returns a fully qualified path name, which may be used in a more human-friendly
	/// manner to find certain named objects within a hierarchy of objects.
	/// </summary>
	public string fqpn {
		get {
			lock( _lock )
			{
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
	}

	public IEnumerable<Mob> contained {
		get {
			int id = this.id;
			return _world.findObjects( (m) => m.get.locationId == id );
		}
	}

	public World world { get { return _world; } }

	public bool isDescendentOf(int id) {
		lock( _lock )
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

	/// <summary>
	/// Little disposable class to allow for external locking of Mobs.
	/// </summary>
	public class Locker : IDisposable
	{
		public Locker( object l )
		{
			_l = l;
			System.Threading.Monitor.Enter( _l );
		}

		public void Dispose()
		{
			if( _l != null )
				System.Threading.Monitor.Exit( _l );
			_l = null;
		}

		object _l;
	}

	/// <summary>
	/// Gets a locker object, automatically locking this Mob from further changes/use.
	/// Make sure that you Dispose the locker.
	/// </summary>
	public Locker getLock()
	{
		return new Locker( _lock );
	}


	// Lock for stuff below.
	object _lock;

	// The reality we belong to.
	World _world;

	// IDs
	int _id;

	// Parent ID (local only)
	int _parentId;

	// Location ID (local only)
	int _locationId;

	// Permissions mask (local only)
	Perm _perms;

	// Object owner (local only)
	int _ownerId;

	// True if we've changed since the last time.
	bool _changed;

	// Verbs attached to the object
	Dictionary<StringI, Verb> _verbs = new Dictionary<StringI, Verb>();

	// Attributes on the object
	Dictionary<StringI, TypedAttribute> _attributes = new Dictionary<StringI,TypedAttribute>();
}

}
