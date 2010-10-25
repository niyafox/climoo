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
	internal MobProxy(Mob mob) {
		_mob = mob;
	}

	static string[] passThroughMembers = {
		"id",
		"parentId",
		"parent",
		"locationId",
		"location",
		"desc"
	};

	public int id { get { return _mob.id; } }

	public int parentId { get { return _mob.parentId; } }
	public MobProxy parent {
		get {
			return new MobProxy(_mob.world.findObject(_mob.parentId));
		}
	}

	public int locationId { get { return _mob.locationId; } }
	public MobProxy location {
		get {
			return new MobProxy(_mob.world.findObject(_mob.locationId));
		}
	}

	public string desc { get { return _mob.desc; } }

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

	public void verbExec(string verbId, string[] parameters) {
	}

	Mob _mob;

	public bool isMemberPassthrough(string name) {
		return passThroughMembers.Contains(name);
	}

	public object getMember(string name) {
		throw new NotImplementedException();
	}

	public string getMimeType(string name) {
		throw new NotImplementedException();
	}

	public bool hasMember(string name) {
		throw new NotImplementedException();
	}

	public IEnumerable<string> getMemberNames() {
		throw new NotImplementedException();
	}

	public void setMember(string name, object val) {
		throw new NotImplementedException();
	}

	public void setMimeType(string name, string type) {
		throw new NotImplementedException();
	}

	public bool hasMethod(string name, object[] args) {
		throw new NotImplementedException();
	}

	public bool isMethodPassthrough(string name) {
		return false;
	}

	public object callMethod(string name, object[] args) {
		throw new NotImplementedException();
	}
}

}
