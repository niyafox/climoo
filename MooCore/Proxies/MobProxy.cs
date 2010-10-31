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
public class MobProxy : IDynamicObject {
	internal MobProxy(Mob mob, Player player) {
		_mob = mob;
		_player = player;
	}

	static string[] passThroughMembers = {
		"id",
		"parentId",
		"parent",
		"locationId",
		"location",
		"sentient",
		"desc",
		"fqpn",
		"contained"
	};
	static string[] passThroughMethods = {
		"Equals",
		"IsTrue"
	};

	public int id { get { return _mob.id; } }

	public int parentId { get { return _mob.parentId; } }
	public MobProxy parent {
		get {
			return new MobProxy(_mob.world.findObject(_mob.parentId), _player);
		}
	}

	public int locationId { get { return _mob.locationId; } }
	public MobProxy location {
		get {
			return new MobProxy(_mob.world.findObject(_mob.locationId), _player);
		}
	}

	public string desc { get { return _mob.desc; } }

	public bool sentient { get { return _mob.isDescendentOf(_mob.world.findObject(":templates:player").id); } }

	public string fqpn { get { return _mob.fqpn; } }

	public MobProxy[] contained {
		get {
			return (from m in _mob.contained select new MobProxy(m, _player)).ToArray();
		}
	}

	public string attrAsString(string id) {
		object val = _mob.findAttribute(id);
		if (val is string)
			return val as string;
		else if (val is TypedAttribute) {
			var ta = val as TypedAttribute;
			if (ta.mimetype.StartsWith("image/") && _mob.world.attributeUrlGenerator != null)
				return string.Format("[img]{0}[/img]", _mob.world.attributeUrlGenerator(_mob, id));
			else
				return "<binary blob>";
		} else
			return null;
	}

	public void attrSet(string id, string val) {
		_mob.attributes[id] = val;
	}

	public void attrDel(string id) {
		if (_mob.attributes.ContainsKey(id))
			_mob.attributes.Remove(id);
	}

	public object verbExec(string verbId, string[] parameters) {
		/* var verb = _mob.findVerb(verbId);
		if (verb == null)
			throw new ArgumentException("Invalid verb name");
		return verb.invoke(string.Format("{0} {1}", verbId, string.Join(" ", parameters)),
			_mob,
			_player); */
		throw new NotImplementedException();
	}

	// Because of the way the S# runtime works, this allows us to override ==.
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
	public bool IsTrue() {
		return _mob.id != Mob.None.id;
	}

	Mob _mob;
	Player _player;


	public virtual bool isMemberPassthrough(string name) {
		return passThroughMembers.Contains(name);
	}

	public virtual object getMember(string name) {
		return attrAsString(name);
	}

	public virtual string getMimeType(string name) {
		throw new NotImplementedException();
	}

	public virtual bool hasMember(string name) {
		return _mob.findAttribute(name) != null;
	}

	public virtual IEnumerable<string> getMemberNames() {
		throw new NotImplementedException();
	}

	public virtual void setMember(string name, object val) {
		_mob.attributes[name] = val;
	}

	public virtual void setMimeType(string name, string type) {
		throw new NotImplementedException();
	}

	public virtual bool hasMethod(string name) {
		return false;
	}

	public virtual bool isMethodPassthrough(string name) {
		return passThroughMethods.Contains(name);
	}

	public virtual object callMethod(Scope scope, string name, object[] args) {
		return verbExec(name, (from x in args select x.ToString()).ToArray());
	}
}

}
