namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class Verb {
	public Verb() { }

	public string command { get; set; }
	public string help { get; set; }
	public string code {
		get {
			if (_script != null)
				return _script.code;
			else
				return null;
		}
		set {
			if (_script == null)
				_script = new ScriptFragment(value);
			else
				_script.code = value;
		}
	}

	public void invoke(string inputLine, Mob self, Proxies.UserProxy user) {
		var proxy = new Proxies.MobProxy(self);
		var scope = new Scope();
		scope.set("inputLine", inputLine);
		scope.set("self", proxy);
		scope.set("user", user);
		_script.execute(scope);
	}

	ScriptFragment _script;
}

}
