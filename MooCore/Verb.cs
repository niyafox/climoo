namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class Verb {
	public Verb() {
		this.help = "";
	}

	/// <summary>
	/// Prepositional phrase choices.
	/// </summary>
	public enum Prep {
		None,		// Slot not allowed
		Ambiguous,	// Ambiguous match
		Any,		// Match any preposition
		Wildcard,	// Like None+Any

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

	// Input-parsable prepositional phrases.
	static Dictionary<Prep, string[]> Alternates = new Dictionary<Prep,string[]> {
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
		{ Prep.Off,			new[] { "off", "off of", "away from" } }
	};

	/// <summary>
	/// Parse a full prepositional phrase, as a single string.
	/// </summary>
	/// <param name="s">The phrase</param>
	/// <returns>The matching preposition</returns>
	/// <exception cref="ArgumentException">May be thrown if the phrase doesn't match</exception>
	/// <remarks>
	/// This is more or less intended as a way to do Enum.Parse(Prep).
	/// </remarks>
	static public Prep ParsePrep(string s) {
		s = s.ToLowerInvariant();
		if (s == "none")
			return Prep.None;
		else if (s == "any")
			return Prep.Any;
		else if (s == "*" || s == "wildcard")
			return Prep.Wildcard;

		foreach (var p in Alternates)
			if (p.Value.ContainsI(s))
				return p.Key;
		throw new ArgumentException("Invalid preposition string '" + s + "'.");
	}

	public struct PrepMatch {
		public PrepMatch(Prep p, IEnumerable<string> w) {
			this.prep = p;
			this.words = w;
		}
		public Prep prep;
		public IEnumerable<string> words;

		public bool isReal { get { return this.prep != Prep.None && this.prep != Prep.Ambiguous; } }

		static public PrepMatch None = new PrepMatch(Prep.None, null);
		static public PrepMatch Ambiguous = new PrepMatch(Prep.Ambiguous, null);
	}

	/// <summary>
	/// Attempt to match a full set of words to a prepositional phrase.
	/// </summary>
	/// <param name="tokens">The words</param>
	/// <returns>A matching preposition, or None, or Ambiguous.</returns>
	static public PrepMatch MatchPrep(IEnumerable<string> tokens) {
		if (tokens.Count() == 1 && tokens.First().EqualsI("none"))
			return PrepMatch.None;

		MakeMatches();

		var answers = s_matches.findMatches(from t in tokens select new StringI(t));
		if (answers.Count() == 0)
			return PrepMatch.None;
		if (answers.Count() > 1)
			return PrepMatch.Ambiguous;

		var answer = answers.First();
		return new PrepMatch(answer.match.p, from w in answer.path select (string)w);
	}

	/// <summary>
	/// Object specifier choices.
	/// </summary>
	public enum Specifier {
		/// <summary>
		/// May only match the object containing the verb
		/// </summary>
		Self,

		/// <summary>
		/// May match any existing object
		/// </summary>
		Any,

		/// <summary>
		/// Matches only Mob.None
		/// </summary>
		None,

		/// <summary>
		/// A "don't care" value -- like Any+None.
		/// </summary>
		Wildcard
	}

	/// <summary>
	/// Parse an object specifier.
	/// </summary>
	/// <param name="s">The specifier</param>
	/// <returns>The matching specifier type</returns>
	/// <exception cref="ArgumentException">May be thrown if the string doesn't match</exception>
	/// <remarks>
	/// This is more or less intended as a way to do Enum.Parse(Specifier).
	/// </remarks>
	static public Specifier ParseSpecifier(string s) {
		if (s.EqualsI("none"))
			return Specifier.None;
		else if (s.EqualsI("self"))
			return Specifier.Self;
		else if (s.EqualsI("any"))
			return Specifier.Any;
		else if (s.EqualsI("wildcard") || s == "*")
			return Specifier.Wildcard;
		else
			throw new ArgumentException("Invalid specifier string '" + s + "'.");
	}

	/// <summary>
	/// A full verb method signature. Specifies a template for verb matching.
	/// </summary>
	public class Sig {
		public Specifier dobj = Specifier.None;
		public Prep prep = Prep.None;
		public Specifier iobj = Specifier.None;
		public Prep prep2 = Prep.None;
		public Specifier iobj2 = Specifier.None;

		/// <summary>
		/// If true, this verb may match based solely on the verb name, if it is
		/// located on the player or the player's location. This is to allow things
		/// like "say" and "emote". This is more or less equivalent to "verb * * * * *"
		/// but saves some processing time and avoids edge cases where words passed
		/// to the verb might match an object name.
		/// </summary>
		public bool wildcard = false;
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
			if (code.TrimStart().StartsWithI("//verb"))
				parseForSignatures();
		}
	}

	void parseForSignatures() {
		// Split the input into lines, and weed out only the ones with sig values.
		IEnumerable<string> verbLines = _script.code
			.Split('\n')
			.Select(l => l.Trim())
			.Where(l => l.TrimStart().StartsWithI("//verb"));

		// Process each one into a sig...
		List<Sig> newSigs = new List<Sig>();
		foreach (string verbLine in verbLines) {
			string[] p = verbLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			Sig sig = new Sig();
			if (p[0].EqualsI("//verb*"))
				sig.wildcard = true;
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
		public string[]	inputwords = new string[0];
		public Mob		caller = null;			// If different from player
		public Mob		self = Mob.None;
		public Mob		dobj = Mob.None;
		public Prep		prep = Prep.None;
		public Mob		iobj = Mob.None;
		public Prep		prep2 = Prep.None;
		public Mob		iobj2 = Mob.None;
		public string[]	dobjwords = null;
		public string[]	prepwords = null;
		public string[]	iobjwords = null;
		public string[]	prep2words = null;
		public string[]	iobj2words = null;
		public Player	player = null;
		public object[]	args = new object[0];	// If a call from another script

		/// <summary>
		/// Generates a memberwise shallow clone.
		/// </summary>
		public VerbParameters clone() {
			return (VerbParameters)this.MemberwiseClone();
		}
	}

	public IEnumerable<Sig> match(VerbParameters param) {
		return
			from s in this.signatures
			where MatchSig(param, s)
			select s;
	}

	public IEnumerable<Sig> matchWildcards(VerbParameters param) {
		return
			from s in this.signatures
			where s.wildcard && MatchSig(param, s)
			select s;
	}

	static bool MatchSig(VerbParameters param, Sig s) {
		if (s.wildcard)
			return true;

		return MatchObj(param, param.dobj, s.dobj)
			&& MatchPrep(param.prep, s.prep)
			&& MatchObj(param, param.iobj, s.iobj)
			&& MatchPrep(param.prep2, s.prep2)
			&& MatchObj(param, param.iobj2, s.iobj2);
	}

	static bool MatchPrep(Prep a, Prep b) {
		if (a == Prep.Wildcard || b == Prep.Wildcard)
			return true;
		if ((a == Prep.Any || b == Prep.Any)
			&& (a != Prep.None && b != Prep.None))
		{
			return true;
		}
		if ((a == Prep.None || b == Prep.None)
			&& (a != Prep.None || b != Prep.None))
		{
			return false;
		}

		return a == b;
	}

	static bool MatchObj(VerbParameters param, Mob m, Specifier spec) {
		if (spec == Specifier.Wildcard)
			return true;
		if (spec == Specifier.Any)
			return m != null && m != Mob.None;
		if (spec == Specifier.Self)
			return m != null && m.id == param.self.id;

		if (spec == Specifier.None
			&& (m == null || m == Mob.None))
		{
			return true;
		}

		return false;
	}

	public const string VerbParamsKey = "verbparams";
	public object invoke(VerbParameters param) {
		var scope = new Scope();
		scope.set("input", param.input);
		scope.set("inputwords", param.inputwords);
		scope.set("self", new Proxies.MobProxy(param.self, param.player));
		scope.set("obj", new Proxies.MobProxy(param.dobj, param.player));
		if (param.prep != Prep.None)
			scope.set("prep", param.prep.ToString().ToLowerInvariant());
		else
			scope.set("prep", null);
		scope.set("indobj", new Proxies.MobProxy(param.iobj, param.player));
		if (param.prep2 != Prep.None)
			scope.set("prep2", param.prep2.ToString().ToLowerInvariant());
		else
			scope.set("prep2", null);
		scope.set("indobj2", new Proxies.MobProxy(param.iobj2, param.player));

		scope.set("objwords", param.dobjwords);
		scope.set("prepwords", param.prepwords);
		scope.set("indobjwords", param.iobjwords);
		scope.set("prep2words", param.prep2words);
		scope.set("indobj2words", param.iobj2words);

		scope.set("ambiguous", Proxies.MobProxy.Ambiguous);
		scope.set("none", Proxies.MobProxy.None);

		Proxies.PlayerProxy player = null;
		if (param.player != null)
			player = new Proxies.PlayerProxy(param.player);
		scope.set("player", player);

		// "caller" is the same as the player, unless otherwise specified.
		if (param.caller != null)
			scope.set("caller", new Proxies.MobProxy(param.caller, param.player));
		else
			scope.set("caller", player);

		scope.set("args", param.args);
		scope.set("world", new Proxies.WorldProxy(param.player.avatar.world, param.player));
		scope.set("$", new Proxies.MobProxy(param.player.avatar.world.findObject(1), param.player));

		scope.queryForItem = (name) => {
			if (name.StartsWithI("#")) {
				int number = CultureFree.ParseInt(name.Substring(1));
				return new Proxies.MobProxy(param.player.avatar.world.findObject(number), param.player);
			}
			return null;
		};

		// Pass these on literally to any down-stream invokes.
		scope.baggageSet(VerbParamsKey, param);

		return _script.execute(scope);
	}

	ScriptFragment _script;

	//////////////////////////////////////////////////////////////
	class PrepWrap {
		public PrepWrap(Prep p) { this.p = p; }
		public Prep p;
	}
	static MatchTree<StringI, PrepWrap> s_matches = null;

	static void MakeMatches() {
		if (s_matches != null)
			return;
		s_matches = new MatchTree<StringI, PrepWrap>();

		foreach (var p in Alternates) {
			foreach (var alt in p.Value) {
				var pieces = from s in alt.Split(' ') select new StringI(s);
				s_matches.addMatch(pieces, new PrepWrap(p.Key));
			}
		}
	}
}

}
