﻿namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// Scripting proxy for Mobs.
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

	[Passthrough]
	public int id { get { return _mob.id; } }

	[Passthrough]
	public int parentId { get { return _mob.parentId; } }

	[Passthrough]
	public MobProxy parent {
		get {
			return new MobProxy(_mob.world.findObject(_mob.parentId), _player);
		}
	}

	[Passthrough]
	public int locationId { get { return _mob.locationId; } }

	[Passthrough]
	public MobProxy location {
		get {
			return new MobProxy(_mob.world.findObject(_mob.locationId), _player);
		}
	}

	[Passthrough]
	public string desc { get { return _mob.desc; } }

	[Passthrough]
	public bool sentient { get { return _mob.isDescendentOf(_mob.world.findObject(":templates:player").id); } }

	[Passthrough]
	public string fqpn { get { return _mob.fqpn; } }

	[Passthrough]
	public MobProxy[] contained {
		get {
			return (from m in _mob.contained select new MobProxy(m, _player)).ToArray();
		}
	}

	[Passthrough]
	public IEnumerable<string> attributes {
		get {
			return _mob.attrList;
		}
	}

	[Passthrough]
	public object attrGet(string id) {
		TypedAttribute ta = _mob.findAttribute(id);
		if (ta == null)
			return null;
		if (ta.isString)
			return ta.str;
		else if (ta.isMobRef)
			return _mob.world.findObject(ta.mobref.id);
		else if (ta.isImage && _mob.world.attributeUrlGenerator != null)
			return string.Format("[img]{0}[/img]", _mob.world.attributeUrlGenerator(_mob, id));
		else
			return "<binary blob>";
	}

	[Passthrough]
	public void attrSet(string id, object val) {
		if (val is Mob)
			val = new Mob.Ref(val as Mob);
		_mob.attrSet(id, val);
	}

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

	// This allows us to do a scripted type coercion, which we'll use to compare
	// against the None object.
	[Passthrough]
	public bool IsTrue() {
		return _mob.id != Mob.None.id;
	}

	[Passthrough]
	public override string ToString() {
		return "<Mob: {0}>".FormatI(this.fqpn);
	}

	Mob _mob;
	Player _player;


	public override object getMember(string name) { return attrGet(name); }
	public override string getMimeType(string name) {
		TypedAttribute ta = _mob.findAttribute(name);
		if (ta == null)
			return null;
		else
			return ta.mimetype;
	}
	public override bool hasMember(string name) { return _mob.findAttribute(name) != null; }
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
		var newparam = new Verb.VerbParameters() {
			input = param.input,
			self = _mob,
			dobj = param.dobj,
			prep = param.prep,
			iobj = param.iobj,
			prep2 = param.prep2,
			iobj2 = param.iobj2,
			player = param.player,
			caller = param.self,
			args = args
		};

		return v.invoke(newparam);
	}
}

}
