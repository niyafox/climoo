namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

class WorldProxy : IDynamicObject {
	public WorldProxy(World w) {
		_w = w;
	}

	World _w;

	public MobProxy get(int id) {
		Mob m = _w.findObject(id);

		throw new NotImplementedException();
	}

	public MobProxy get(string path) {
		throw new NotImplementedException();
	}

	static string[] s_ptmem = new string[0];
	public bool isMemberPassthrough(string name) {
		return (from i in s_ptmem where name.Equals(i, StringComparison.OrdinalIgnoreCase) select 1).Any();
	}

	public object getMember(string name) { throw new NotImplementedException(); }
	public string getMimeType(string name) { throw new NotImplementedException(); }
	public bool hasMember(string name) { return false; }
	public IEnumerable<string> getMemberNames() { return new string[0]; }
	public void setMember(string name, object val) { throw new NotImplementedException(); }
	public void setMimeType(string name, string type) { throw new NotImplementedException(); }

	public bool hasMethod(string name) {
		return false;
	}

	static string[] s_ptmeth = new string[0];
	public bool isMethodPassthrough(string name) {
		return (from i in s_ptmeth where name.Equals(i, StringComparison.OrdinalIgnoreCase) select 1).Any();
	}

	public object callMethod(Scope scope, string name, object[] args) { throw new NotImplementedException(); }
}

}
