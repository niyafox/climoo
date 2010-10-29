namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class Verb {
	public Verb() { }

	public enum Prep {
		None,		// Slot not allowed
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
		Around,
		Off			// off of, away from
	}
	static Dictionary<Prep, string[]> Alternates = new Dictionary<Prep,string[]> {
		{ Prep.None,		new[] { "none" } },
		{ Prep.With,		new[] { "with", "using" } },
		{ Prep.At,			new[] { "at", "to", "toward" } },
		{ Prep.In,			new[] { "in", "inside", "into", "within" } },
		{ Prep.On,			new[] { "on", "on top of", "onto", "upon", "above", "over" } },
		{ Prep.From,		new[] { "from", "out of", "from inside" } },
		{ Prep.Through,		new[] { "through" } },
		{ Prep.Under,		new[] { "under", "underneath", "beneath" } },
		{ Prep.Behind,		new[] { "behind" } },
		{ Prep.InFrontOf,	new[] { "in front of" } },
		{ Prep.Beside,		new[] { "beside" } },
		{ Prep.For,			new[] { "for", "about" } },
		{ Prep.Is,			new[] { "is" } },
		{ Prep.As,			new[] { "as" } },
		{ Prep.Around,		new[] { "around" } },
		{ Prep.Off,			new[] { "off", "off of", "away from " } }
	};
	static public Prep ParsePrep(string s) {
		s = s.ToLower();
		foreach (var p in Alternates)
			if (p.Value.Contains(s))
				return p.Key;
		throw new ArgumentException("Invalid preposition string '" + s + "'.");
	}

	public enum Specifier {
		Self, Any, None
	}
	static public Specifier ParseSpecifier(string s) {
		s = s.ToLower();
		if (s == "none")
			return Specifier.None;
		else if (s == "self")
			return Specifier.Self;
		else if (s == "any")
			return Specifier.Any;
		else
			throw new ArgumentException("Invalid specifier string '" + s + "'.");
	}

	public class Sig {
		public Specifier dobj = Specifier.None;
		public Prep prep = Prep.None;
		public Specifier iobj = Specifier.None;
		public Prep prep2 = Prep.None;
		public Specifier iobj2 = Specifier.None;
	}

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
			// Normalize line endings, if needed.
			string code = value;
			if (code.IndexOf('\r') >= 0) {
				code.Replace("\r\n", "\n");
			}

			if (_script == null)
				_script = new ScriptFragment(code);
			else
				_script.code = code;

			// Are there method signatures at the top in comment form?
			if (code.TrimStart().StartsWith("//verb"))
				parseForSignatures();
		}
	}

	void parseForSignatures() {
		// Split the input into lines, and weed out only the ones with sig values.
		IEnumerable<string> verbLines = _script.code.Split('\n').Where(l => l.StartsWith("//verb"));

		// Process each one into a sig...
		List<Sig> newSigs = new List<Sig>();
		foreach (string verbLine in verbLines) {
			string[] p = verbLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			Sig sig = new Sig();
			if (p.Length > 1) {
				// Direct object.
				sig.dobj = ParseSpecifier(p[1]);
			}
			if (p.Length > 2) {
				// Preposition.
				sig.prep = ParsePrep(p[2]);
			}
			if (p.Length > 3) {
				// Indirect object.
				sig.iobj = ParseSpecifier(p[3]);
			}
			if (p.Length > 4) {
				// Second preposition.
				sig.prep2 = ParsePrep(p[4]);
			}
			if (p.Length > 5) {
				// Second indirect object.
				sig.iobj2 = ParseSpecifier(p[5]);
			}
			newSigs.Add(sig);
		}
		this.signatures = newSigs;
	}

	public IEnumerable<Sig> signatures { get; set; }

	public class VerbParameters {
		public string	input = "";
		public Mob		self = Mob.None;
		public Mob		dobj = Mob.None;
		public string	prep = "";
		public Mob		iobj = Mob.None;
		public string	prep2 = "";
		public Mob		iobj2 = Mob.None;
		public Player	player = null;
	}

	public object invoke(VerbParameters param) {
		var scope = new Scope();
		scope.set("input", param.input);
		scope.set("self", new Proxies.MobProxy(param.self, param.player));
		scope.set("obj", param.dobj);
		scope.set("prep", param.prep);
		scope.set("indobj", param.iobj);
		scope.set("prep2", param.prep2);
		scope.set("inbobj2", param.iobj2);
		scope.set("ambiguous", Mob.Ambiguous);
		scope.set("none", Mob.None);
		if (param.player != null)
			scope.set("player", new Proxies.PlayerProxy(param.player));
		else
			scope.set("player", null);
		return _script.execute(scope);
	}

	ScriptFragment _script;
}

}
