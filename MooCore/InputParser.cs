namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InputParser {
	static public string ProcessInput(string input, Player player) {
		// Split the input.
		string[] pieces = input.Trim().Split(' ', '\t', '\n', '\r');
		if (pieces.Length == 0)
			return "";

		// For now, the verb is always one word.
		string verb = pieces[0];
		if (verb == "\"") verb = "say";
		if (verb == "/me") verb = "emote";

		// For now, the direct object is always the rest of the phrase. Indirect
		// objects aren't allowed.
		string objName = string.Join(" ", pieces.Skip(1).ToArray());

		// Look for objects around the player that might match the next word or two.
		IEnumerable<Mob> objOptions =
			from m in player.avatar.contained
				.Concat(player.avatar.location.contained)
			where m.name.StartsWith(objName)
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

		// No direct objects yet (no prep parsing yet!)
		Mob iobj = Mob.None;

		// Look for a matching verb.
		var selectedVerb = SearchVerbsFrom(verb, player.avatar);
		if (selectedVerb == null)
			selectedVerb = SearchVerbsFrom(verb, player.avatar.location);
		if (selectedVerb == null && dobj != null)
			selectedVerb = SearchVerbsFrom(verb, dobj);
		if (selectedVerb == null && iobj != null)
			selectedVerb = SearchVerbsFrom(verb, iobj);

		// Couldn't find one?
		if (selectedVerb == null)
			return "Sorry, I don't know what that means.";

		// Execute the verb.
		selectedVerb.Item2.invoke(input, "", selectedVerb.Item1, dobj, iobj, player);

		// Any output will come from the script.
		return "";
	}

	static Tuple<Mob,Verb> SearchVerbsFrom(string verbName, Mob m) {
		foreach (var v in m.allVerbs)
			if (v.Value.name == verbName)
				return Tuple.Create(m, v.Value);
		return null;
	}
}

}
