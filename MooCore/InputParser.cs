namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InputParser {
	/// <summary>
	/// Try to find a player-relative object by string using normal lookup
	/// rules as below.
	/// </summary>
	/// <returns>A valid Mob, or one of its constants (None, Ambiguous)</returns>
	static public Mob MatchName(string name, Player player) {
		return ObjectMatch(name, player);
	}

	/// <summary>
	/// Process a line of input from the player: parse and execute any action.
	/// </summary>
	static public string ProcessInput(string input, Player player) {
		// Does the input start with a special character?
		if (input[0] == ';') {
			// Execute this as a chunk of MooScript, as if it was attached
			// to the player.
			return ExecuteImmediate(input, player);
		}
		if (input[0] == '"')
			input = "say " + input.Substring(1);
		if (input[0] == ':')
			input = "emote " + input.Substring(1);
		if (input[0] == '@')
			input = "whisper " + input.Substring(1);

		// Split the input.
		string[] pieces = input.Trim().Split(' ', '\t', '\n', '\r');
		if (pieces.Length == 0)
			return "";

		// For now, the verb is always one word.
		string verb = pieces[0];
		if (verb.EqualsI("/me") || verb == ":") verb = "emote";

		// Skip forward until we find a preposition.
		var remaining = pieces.Skip(1);
		var start = remaining;
		Verb.PrepMatch p = Verb.PrepMatch.None;
		string dobjName = null;
		for (int skip=0; skip<remaining.Count(); ++skip) {
			var chunk = remaining.Skip(skip);
			p = Verb.MatchPrep(chunk);
			if (p.isReal) {
				// Skip over the preposition.
				remaining = chunk.Skip(p.words.Count());
				dobjName = string.Join(" ", start.Take(skip));
				break;
			}
		}

		if (p.prep == Verb.Prep.None) {
			// No preposition -> the rest of the string is the direct object.
			dobjName = string.Join(" ", remaining);
			remaining = new string[0];
		}

		// For now, the indirect object is always the rest of the phrase.
		string iobjName = null;
		if (remaining.Count() > 0) {
			iobjName = string.Join(" ", remaining);
		}

		// Look for objects around the player that might match the direct and indirect objects.
		Mob dobj = ObjectMatch(dobjName, player);
		Mob iobj = ObjectMatch(iobjName, player);

		// Look for a matching verb.
		var param = new Verb.VerbParameters() {
			input = input,
			self = null,
			dobj = dobj,
			prep = p.prep,
			iobj = iobj,
			player = player
		};
		var selectedVerb = SearchVerbsFrom(player.avatar, verb, param);
		if (selectedVerb.Count() == 0)
			selectedVerb = SearchVerbsFrom(player.avatar.location, verb, param);
		if (selectedVerb.Count() == 0 && dobj != null)
			selectedVerb = SearchVerbsFrom(dobj, verb, param);
		if (selectedVerb.Count() == 0 && iobj != null)
			selectedVerb = SearchVerbsFrom(iobj, verb, param);

		// Couldn't find one?
		if (selectedVerb.Count() != 1)
			return "Sorry, I don't know what that means.";

		// Execute the verb.
		var v = selectedVerb.First();
		param.self = v.Item1;
		v.Item2.invoke(param);

		// Any output will come from the script.
		return "";
	}

	/// <summary>
	/// Execute the input as if it were a tiny MooScript verb attached to the player.
	/// </summary>
	/// <returns>
	/// If the MooScript returns a value, it will be sent back to the player.
	/// </returns>
	static public string ExecuteImmediate(string input, Player player) {
		Verb v = new Verb() {
			name = "inline",
			help = "",
			code = input.Substring(1) + ';'
		};

		var param = new Verb.VerbParameters() {
			input = input,
			self = player.avatar,
			dobj = Mob.None,
			prep = Verb.Prep.None,
			iobj = Mob.None,
			player = player
		};

		var rv = v.invoke(param);

		// Try to do some reallly basic type massaging to make it viewable on the terminal.
		string rvs;
		if (rv is string) {
			rvs = "\"{0}\"".FormatI(rv);
		} else if (rv is System.Collections.IEnumerable) {
			rvs = string.Join(", ", (from object i in (System.Collections.IEnumerable)rv select i.ToStringI()));
		} else
			rvs = rv.ToStringI();

		return MooCode.PrepareForClient(rvs);
	}

	static Mob ObjectMatch(string objName, Player player) {
		if (string.IsNullOrEmpty(objName))
			return Mob.None;

		// Adjust any special object names.
		if ("me".EqualsI(objName))
			objName = "#{0}".FormatI(player.avatar.id);
		if ("here".EqualsI(objName))
			objName = "#{0}".FormatI(player.avatar.locationId);

		// If it's a numeric object ID, go ahead and just look it up.
		if (objName.StartsWithI("#"))
			return player.avatar.world.findObject(CultureFree.ParseInt(objName.Substring(1)));

		// If it's an absolute path name, look it up.
		if (objName.StartsWithI(":"))
			return player.avatar.world.findObject(objName);

		// Look in the normal places, otherwise.
		IEnumerable<Mob> objOptions =
			from m in player.avatar.contained
				.Concat(player.avatar.location.contained)
			where m.name.StartsWithI(objName)
			select m;
		IEnumerable<Mob> exactMatches =
			from m in objOptions
			where m.name == objName
			select m;
		Mob dobj = null;
		if (objOptions.Count() == 0)
			dobj = Mob.None;
		else if (objOptions.Count() > 1) {
			if (exactMatches.Count() == 1)
				dobj = exactMatches.First();
			else
				dobj = Mob.Ambiguous;
		} else
			dobj = objOptions.First();

		return dobj;
	}

	static IEnumerable<Tuple<Mob,Verb>> SearchVerbsFrom(Mob m, string verbName,
		Verb.VerbParameters param)
	{
		param.self = m;
		foreach (var v in m.allVerbs)
			if (v.Value.item.name == verbName) {
				if (v.Value.item.match(param).Count() > 0)
					yield return Tuple.Create(m, v.Value.item);
			}
	}
}

}
