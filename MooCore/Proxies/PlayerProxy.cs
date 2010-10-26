namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class PlayerProxy : IDynamicObject {
	public PlayerProxy(Player player) {
		_player = player;
	}
	Player _player;

	public void write(string text) {
		_player.write(text);
	}

	public bool isMemberPassthrough(string name) {
		return false;
	}

	public object getMember(string name) {
		throw new DynamicObjectFailure("not implemented");
	}

	public string getMimeType(string name) {
		throw new DynamicObjectFailure("not implemented");
	}

	public bool hasMember(string name) {
		throw new DynamicObjectFailure("not implemented");
	}

	public IEnumerable<string> getMemberNames() {
		throw new DynamicObjectFailure("not implemented");
	}

	public void setMember(string name, object val) {
		throw new DynamicObjectFailure("not implemented");
	}

	public void setMimeType(string name, string type) {
		throw new DynamicObjectFailure("not implemented");
	}

	public bool hasMethod(string name) {
		throw new DynamicObjectFailure("not implemented");
	}

	public bool isMethodPassthrough(string name) {
		return name == "write";
	}

	public object callMethod(Scope scope, string name, object[] args) {
		throw new NotImplementedException();
	}
}

}
