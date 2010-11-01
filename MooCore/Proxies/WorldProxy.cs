namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

class WorldProxy : IDynamicObject {
	public WorldProxy(World w, Player p) {
		_w = w;
		_p = p;
	}

	World _w;
	Player _p;

	public MobProxy obj(int id) {
		Mob m = _w.findObject(id);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
	}

	public MobProxy obj(string path) {
		Mob m = _w.findObject(path);
		if (m == null) m = Mob.None;
		return new MobProxy(m, _p);
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

	static string[] s_ptmeth = new string[] { "obj" };
	public bool isMethodPassthrough(string name) {
		return (from i in s_ptmeth where name.Equals(i, StringComparison.OrdinalIgnoreCase) select 1).Any();
	}

	public object callMethod(Scope scope, string name, object[] args) { throw new NotImplementedException(); }
}

}
