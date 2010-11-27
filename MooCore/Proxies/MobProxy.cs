namespace Kayateia.Climoo.MooCore.Proxies {
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

	static public MobProxy Ambiguous {
		get { return s_ambig; }
	}
	static MobProxy s_ambig = new MobProxy(Mob.Ambiguous, null);

	static public MobProxy None {
		get { return s_none; }
	}
	static MobProxy s_none = new MobProxy(Mob.None, null);

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
	public bool sentient { get { return _mob.isDescendentOf(_mob.world.findObject("/templates/player").id); } }

	[Passthrough]
	public string fqpn { get { return _mob.fqpn; } }

	[Passthrough]
	public PlayerProxy player {
		get {
			if (_mob.player == null)
				return null;
			else
				return new PlayerProxy(_mob.player);
		}
	}

	[Passthrough]
	public MobProxy[] contained {
		get {
			return (from m in _mob.contained select new MobProxy(m, _player)).ToArray();
		}
	}

	[Passthrough]
	public IEnumerable<AttrProxy> attributes {
		get {
			return from a in _mob.allAttrs select new AttrProxy(a.Value, _player);
		}
	}

	[Passthrough]
	public IEnumerable<VerbProxy> verbs {
		get {
			return from a in _mob.allVerbs select new VerbProxy(a.Value, _player);
		}
	}

	[Passthrough]
	public bool hasVerb(string verb) {
		return _mob.findVerb(verb) != null;
	}

	[Passthrough]
	public void verbDel(string verb) {
		_mob.verbDel(verb);
	}

	[Passthrough]
	public string verbGet(string verb) {
		// TODO: Should this be a new VerbProxy?
		return _mob.verbGet(verb).code;
	}

	[Passthrough]
	public void verbSet(string verb, string code) {
		MooCore.Verb v = new MooCore.Verb() {
			name = verb,
			code = code
		};
		_mob.verbSet(verb, v);
	}

	[Passthrough]
	public object attrGet(string id) {
		TypedAttribute ta = _mob.findAttribute(id);
		if (ta == null)
			return null;
		if (ta.isString)
			return ta.str;
		else if (ta.isMobRef)
			return new MobProxy(_mob.world.findObject(ta.mobref.id), _player);
		else if (ta.isImage && _mob.world.attributeUrlGenerator != null)
			return string.Format("[img]{0}[/img]", _mob.world.attributeUrlGenerator(_mob, id));
		else
			return "<binary blob>";
	}

	[Passthrough]
	public void attrSet(string id, object val) {
		// This shouldn't be possible, but best be prepared..
		if (val is Mob)
			val = new Mob.Ref(val as Mob);
		else if (val is MobProxy)
			val = new Mob.Ref((val as MobProxy).id);
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

	[Passthrough]
	public void moveTo(MobProxy target) {
		_mob.locationId = target._mob.id;
	}
	public void moveTo(int targetId) {
		_mob.locationId = targetId;
	}
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

	// Matches a (possibly user-typed) name relative to the object in question.
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

	// This allows us to do a scripted type coercion, which we'll use to compare
	// against the None object.
	[Passthrough]
	public bool IsTrue() {
		return _mob.id != Mob.None.id;
	}

	[Passthrough]
	public override string ToString() {
		string name = "";
		if (!string.IsNullOrEmpty(_mob.name))
			name = " ({0})".FormatI(_mob.name);
		return "<Mob: {0}{1}>".FormatI(this.fqpn, name);
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
		var newparam = param.clone();
		newparam.self = _mob;
		newparam.caller = param.self;
		newparam.args = args;

		return v.invoke(newparam);
	}
}

}
