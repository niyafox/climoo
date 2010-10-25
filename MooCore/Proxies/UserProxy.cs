namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class UserProxy : IDynamicObject {
	public delegate void NewOutputToSend(string text);
	public event NewOutputToSend NewOutput;

	public void print(string text) {
		if (this.NewOutput != null)
			this.NewOutput(text);
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

	public bool hasMethod(string name, object[] args) {
		throw new DynamicObjectFailure("not implemented");
	}

	public bool isMethodPassthrough(string name) {
		return name == "print";
	}

	public object callMethod(string name, object[] args) {
		throw new DynamicObjectFailure("not implemented");
	}
}

}
