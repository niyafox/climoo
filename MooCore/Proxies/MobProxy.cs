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
/// Scripting proxy for Mobs (MOO objects). This is available to MOO scripts.
/// </summary>
/// <remarks>
/// Everything in here that we provide access to, random scripts also
/// have access to. So this needs to provide security as well.
/// </remarks>
public class MobProxy : DynamicObjectBase {
	internal MobProxy(Mob mob, Player player) {
		_mob = mob;
		_player = player;
	}

	/// <summary>
	/// Returns a copy of the Ambiguous MOO object, for processing text input.
	/// </summary>
	static public MobProxy Ambiguous {
		get { return s_ambig; }
	}
	static MobProxy s_ambig = new MobProxy(Mob.Ambiguous, null);

	/// <summary>
	/// Returns a copy of the None MOO object, for processing text input.
	/// </summary>
	static public MobProxy None {
		get { return s_none; }
	}
	static MobProxy s_none = new MobProxy(Mob.None, null);

	/// <summary>
	/// The mob's world ID.
	/// </summary>
	[Passthrough]
	public int id { get { return _mob.id; } }

	/// <summary>
	/// The mob's parent's world ID. This is the mob's OOP inheritance base.
	/// </summary>
	[Passthrough]
	public int parentId {
		get { return _mob.parentId; }
		set { _mob.parentId = value; }
	}

	/// <summary>
	/// The mob's parent mob. This is the mob's OOP inheritance base.
	/// </summary>
	[Passthrough]
	public MobProxy parent {
		get {
			return new MobProxy(_mob.world.findObject(_mob.parentId), _player);
		}
		set {
			_mob.parentId = value.id;
		}
	}

	/// <summary>
	/// The mob's location's world ID.
	/// </summary>
	[Passthrough]
	public int locationId {
		get { return _mob.locationId; }
		set { _mob.locationId = value; }
	}

	/// <summary>
	/// The mob's location mob.
	/// </summary>
	[Passthrough]
	public MobProxy location {
		get {
			return new MobProxy(_mob.world.findObject(_mob.locationId), _player);
		}
		set {
			_mob.locationId = value.id;
		}
	}

	/// <summary>
	/// The mob's description.
	/// </summary>
	[Passthrough]
	public string desc {
		get { return _mob.desc; }
		set { _mob.desc = value; }
	}

	/// <summary>
	/// Returns true if the mob represents a sentient object, e.g. a player.
	/// </summary>
	[Passthrough]
	public bool sentient { get { return _mob.isDescendentOf(_mob.world.findObject("/templates/player").id); } }

	/// <summary>
	/// Returns the mob's fully qualified path name.
	/// </summary>
	[Passthrough]
	public string fqpn { get { return _mob.fqpn; } }

	/// <summary>
	/// The mob's owner's mob. This is for permission purposes.
	/// </summary>
	[Passthrough]
	public MobProxy owner {
		get {
			return new MobProxy(_mob.owner, _player);
		}
		set {
			// TODO: Only allow mayors to do this.
			_mob.ownerId = value.id;
		}
	}

	/// <summary>
	/// The mob's permission bitmask.
	/// </summary>
	[Passthrough]
	public int perms {
		get {
			return _mob.perms;
		}
		set {
			// TODO: Only allow certain bit changes, unless a mayor.
			_mob.perms = value;
		}
	}

	/// <summary>
	/// If this mob represents a player, this returns a Player object for it. Otherwise, returns null.
	/// </summary>
	[Passthrough]
	public PlayerProxy player {
		get {
			if (_mob.player == null)
				return null;
			else
				return new PlayerProxy(_mob.player);
		}
	}

	/// <summary>
	/// Returns an array of all mobs contained within this mob.
	/// </summary>
	[Passthrough]
	public MobProxy[] contained {
		get {
			return (from m in _mob.contained select new MobProxy(m, _player)).ToArray();
		}
	}

	/// <summary>
	/// Returns all of the attributes of this mob.
	/// </summary>
	[Passthrough]
	public IEnumerable<AttrProxy> attributes {
		get {
			return from a in _mob.allAttrs select new AttrProxy(a.Value, _player);
		}
	}

	/// <summary>
	/// Returns all of the verbs of this mob.
	/// </summary>
	[Passthrough]
	public IEnumerable<VerbProxy> verbs {
		get {
			return from a in _mob.allVerbs select new VerbProxy(a.Value, _player);
		}
	}

	/// <summary>
	/// Returns true if the mob has the specified verb.
	/// </summary>
	[Passthrough]
	public bool hasVerb(string verb) {
		return _mob.findVerb(verb) != null;
	}

	/// <summary>
	/// Deletes the specified verb from the mob.
	/// </summary>
	[Passthrough]
	public void verbDel(string verb) {
		_mob.verbDel(verb);
	}

	/// <summary>
	/// Gets the specified verb's program code if it exists on the mob.
	/// </summary>
	[Passthrough]
	public string verbGet(string verb) {
		// TODO: Should this be a new VerbProxy?
		return _mob.verbGet(verb).code;
	}

	/// <summary>
	/// Sets the specified verb's program code on the mob.
	/// </summary>
	[Passthrough]
	public void verbSet(string verb, string code) {
		MooCore.Verb v = new MooCore.Verb() {
			name = verb,
			code = code
		};
		_mob.verbSet(verb, v);
	}

	/// <summary>
	/// Gets the specified attribute on the mob, if it exists.
	/// </summary>
	[Passthrough]
	public AttrProxy attrGet(string id) {
		TypedAttribute attr = _mob.findAttribute(id); 
		if (attr == null)
			return null;
		else
			return new AttrProxy(attr, _player);
	}

	/// <summary>
	/// Gets the "unboxed" version of the attribute's value. This skips over returning
	/// an attribute proxy, and simply returns the value. This will generate an image URL
	/// if the attribute is an image.
	/// </summary>
	[Passthrough]
	public object attrUnboxed(string id) {
		TypedAttribute ta = _mob.findAttribute(id);
		if (ta == null)
			return null;
		if (ta.isString)
			return ta.str;
		else if (ta.isMobRef)
			return new MobProxy(_mob.world.findObject(ta.mobref.id), _player);
		else if( ta.isMobRefs )
			return ta.mobrefs.Select( mr => new MobProxy( _mob.world.findObject( mr.id ), _player ) ).ToArray();
		else if (ta.isImage && _mob.world.attributeUrlGenerator != null)
			return string.Format("[img]{0}[/img]", _mob.world.attributeUrlGenerator(_mob, id));
		else
			return ta.contents;
	}

	/// <summary>
	/// Sets the attribute value.
	/// </summary>
	[Passthrough]
	public void attrSet(string id, object val) {
		// This shouldn't be possible, but best be prepared..
		if (val is Mob)
			val = new Mob.Ref(val as Mob);
		else if (val is MobProxy)
			val = new Mob.Ref((val as MobProxy).id);
		else if( val is object[] )
		{
			// We need to worry about MobProxies ending up as parts of arrays.
			object[] arr = (object[])val;
			object[] outarr = new object[arr.Length];
			for( int i=0; i<arr.Length; ++i )
			{
				if( arr[i] is Mob )
					outarr[i] = new Mob.Ref( (Mob)arr[i] );
				else if( arr[i] is MobProxy )
					outarr[i] = new Mob.Ref( ((MobProxy)arr[i]).id );
				else
					outarr[i] = arr[i];
			}
			val = outarr;
		}
		_mob.attrSet(id, val);
	}

	/// <summary>
	/// Deletes the specified attribute, if it exists.
	/// </summary>
	[Passthrough]
	public void attrDel(string id) {
		_mob.attrDel(id);
	}

	// Because of the way the S# runtime works, this allows us to override ==.
	[Passthrough]
	public override bool Equals(object obj) {
		if (obj == null || !(obj is MobProxy))
			return false;

		return _mob.id == (obj as MobProxy)._mob.id;
	}

	public override int GetHashCode() {
		return _mob.id;
	}

	/// <summary>
	/// Relocates this mob to another location mob.
	/// </summary>
	[Passthrough]
	public void moveTo(MobProxy target) {
		_mob.locationId = target._mob.id;
	}

	/// <summary>
	/// Relocates this mob to another location mob, by id.
	/// </summary>
	[Passthrough]
	public void moveTo(int targetId) {
		_mob.locationId = targetId;
	}

	/// <summary>
	/// Relocates this mob to another location mob, by path.
	/// </summary>
	[Passthrough]
	public MobProxy moveTo(string targetName) {
		Mob m = InputParser.MatchName(targetName, _mob);
		if (m == null || m == Mob.None) {
			return MobProxy.None;
		} else if (m == Mob.Ambiguous)
			return MobProxy.Ambiguous;
		else {
			moveTo(m.id);
			return new MobProxy(m, _player);
		}
	}

	/// <summary>
	/// Matches a (possibly user-typed) name relative to the object in question.
	/// </summary>
	[Passthrough]
	public MobProxy matchName(string name) {
		Mob m = InputParser.MatchName(name, _mob);
		if (m == null || m == Mob.None)
			return MobProxy.None;
		else if (m == Mob.Ambiguous)
			return MobProxy.Ambiguous;
		else
			return new MobProxy(m, _player);
	}

	/// <summary>
	/// This allows us to do a scripted type coercion, which we'll use to compare
	/// against the None object.
	/// </summary>
	[Passthrough]
	public bool IsTrue() {
		return _mob.id != Mob.None.id;
	}

	/// <summary>
	/// Returns a human-readable string representing this mob.
	/// </summary>
	[Passthrough]
	public override string ToString() {
		string name = "";
		if (!string.IsNullOrEmpty(_mob.name))
			name = " ({0})".FormatI(_mob.name);
		return "<Mob: {0}{1}>".FormatI(this.fqpn, name);
	}

	Mob _mob;
	Player _player;


	////////////////////////////////////////////////////////////////////////////////
	// These methods implement the dynamic scripting interface, which allows us
	// to interpret and approve arbitrary calls to the object from script fragments.
	// This allows us to do things like call verbs on objects as methods, and
	// retrieve attributes like members.

	public override object getMember(string name) { return attrUnboxed(name); }
	public override string getMimeType(string name) {
		TypedAttribute ta = _mob.findAttribute(name);
		if (ta == null)
			return null;
		else
			return ta.mimetype;
	}
	public override bool hasMember(string name) {
		// We do this so that arbitrary attribute names can be resolved to null.
		return true;
	}
	public override IEnumerable<string> getMemberNames() { return _mob.attrList; }
	public override void setMember(string name, object val) { attrSet(name, val); }

	public override void setMimeType(string name, string type) {
		if (!_mob.attrHas(name))
			throw new ArgumentException("Unknown attribute {0}.".FormatI(name));
		var attr = _mob.attrGet(name);
		attr.mimetype = type;
	}

	public override bool hasMethod(string name) {
		Verb v = _mob.findVerb(name);
		return v != null;
	}

	public override object callMethod(Scope scope, string name, object[] args) {
		// Make sure there's a matching verb to be found. Unlike the input
		// parser, this pays no attention to verb signatures.
		Verb v = _mob.findVerb(name);
		if (v == null)
			throw new NotImplementedException("No verb named '" + name + "'.");

		// Look for the previous verb parameters.
		Verb.VerbParameters param = (Verb.VerbParameters)scope.baggageGet(Verb.VerbParamsKey);

		// Make a new one based on it. Most of this will stay the same.
		var newparam = param.clone();
		newparam.self = _mob;
		newparam.caller = param.self;
		newparam.args = args;

		return v.invoke(newparam);
	}
}

}
