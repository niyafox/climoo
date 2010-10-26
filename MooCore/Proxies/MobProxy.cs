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
		"desc"
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

	public string attrGetAsString(string id) {
		object val = _mob.findAttribute(id);
		if (val == null || !(val is string))
			return null;
		return val as string;
	}

	public void attrSetAsString(string id, string val) {
		_mob.attributes[id] = val;
	}

	public void attrDel(string id) {
		if (_mob.attributes.ContainsKey(id))
			_mob.attributes.Remove(id);
	}

	public object verbExec(string verbId, string[] parameters) {
		var verb = _mob.findVerb(verbId);
		if (verb == null)
			throw new ArgumentException("Invalid verb name");
		return verb.invoke(string.Format("{0} {1}", verbId, string.Join(" ", parameters)),
			_mob,
			_player);
	}

	Mob _mob;
	Player _player;


	public bool isMemberPassthrough(string name) {
		return passThroughMembers.Contains(name);
	}

	public object getMember(string name) {
		return _mob.findAttribute(name);
	}

	public string getMimeType(string name) {
		throw new NotImplementedException();
	}

	public bool hasMember(string name) {
		return _mob.findAttribute(name) != null;
	}

	public IEnumerable<string> getMemberNames() {
		throw new NotImplementedException();
	}

	public void setMember(string name, object val) {
		_mob.attributes[name] = val;
	}

	public void setMimeType(string name, string type) {
		throw new NotImplementedException();
	}

	public bool hasMethod(string name) {
		throw new NotImplementedException();
	}

	public bool isMethodPassthrough(string name) {
		return false;
	}

	public object callMethod(Scope scope, string name, object[] args) {
		return verbExec(name, (from x in args select x.ToString()).ToArray());
	}
}

}
