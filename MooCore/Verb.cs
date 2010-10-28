namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class Verb {
	public Verb() { }

	// Not doing all of this yet.
	/* public class Prep {
		public string text;
		public enum Standard {
			With,		// using
			At,			// to, toward
			In,			// inside, into, within
			On,			// on top of, onto, upon, above, over
			From,		// out of, from inside
			Through,
			Under,		// underneath, beneath
			Behind,
			InFrontOf,
			Beside,
			For,		// about
			Is,
			As,
			Off			// off of, away from
		}
		public Standard standard;
	}

	public class Sig {
		public enum Specifier {
			Self, Any, None
		}

		public string name;
		public Specifier @object;
		public Prep prep;

	} */

	public string name { get; set; }
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

	public object invoke(string inputLine, string preposition, Mob self, Mob directObject, Mob indirectObject, Player player) {
		var scope = new Scope();
		scope.set("input", inputLine);
		scope.set("self", new Proxies.MobProxy(self, player));
		scope.set("prep", preposition);
		scope.set("obj", directObject);
		scope.set("indobj", indirectObject);
		scope.set("ambiguous", Mob.Ambiguous);
		scope.set("none", Mob.None);
		if (player != null)
			scope.set("player", new Proxies.PlayerProxy(player));
		else
			scope.set("player", null);
		return _script.execute(scope);
	}

	ScriptFragment _script;
}

}
