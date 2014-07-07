/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Parses commands typed by the player into their console.
/// </summary>
public class InputParser {
	/// <summary>
	/// Try to find a player- (or mob-) relative object by string using normal lookup
	/// rules as below.
	/// </summary>
	/// <returns>A valid Mob, or one of its constants (None, Ambiguous)</returns>
	static public Mob MatchName(string name, Mob refObj) {
		return ObjectMatch(name, refObj);
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

		// Start gathering values for the verb call.
		var param = new Verb.VerbParameters() {
			input = input,
			inputwords = pieces,
			self = null,
			world = player.avatar.world,
			player = player
		};

		// Try for a #1._processInput verb first. If one exists and it returns anything
		// besides false, we'll let it deal with everything else.
		var root = player.avatar.world.findObject(1);
		Verb rootProcess = root.findVerb("_processInput");
		if (rootProcess != null) {
			param.self = root;
			param.caller = player.avatar;
			try {
				object results = rootProcess.invoke(param);
				if (results == null || (results is bool && (bool)results == false)) {
					// Proceed jolly onwards...
				} else {
					// Our work here is finished.
					return "";
				}
			} catch (Exception) {
				// Just assume it didn't handle it. Sucks to get in a loop here if you
				// mess up your global handler..!
				//
				// TODO: Log something out here or print to the player's console.
			}
			param.self = null;
			param.caller = null;
		}

		// Look for complete wildcard verbs on the player and in the player's location.
		// If we find one that matches, stop processing anything else.
		var selectedVerb = SearchWildcardVerbsFrom(player.avatar, verb, param);
		if (!selectedVerb.Any())
			selectedVerb = SearchWildcardVerbsFrom(player.avatar.location, verb, param);
		if (selectedVerb.Any()) {
			var v = selectedVerb.First();
			param.self = v.Item1;
			v.Item2.invoke(param);
			return "";
		}

		// Skip forward until we find a preposition.
		var remaining = pieces.Skip(1);
		var start = remaining;
		Verb.PrepMatch p = Verb.PrepMatch.None;
		string dobjName = null;
		for (int skip=0; skip<remaining.Count(); ++skip) {
			var chunk = remaining.Skip(skip);
			p = Verb.MatchPrep(chunk);
			if (p.isReal) {
				// Skip over the preposition after saving its words.
				param.prepwords = p.words.ToArray();
				remaining = chunk.Skip(param.prepwords.Length);

				// What came before it is the direct object.
				param.dobjwords = start.Take(skip).ToArray();
				dobjName = string.Join(" ", param.dobjwords);
				break;
			}
		}

		if (p.prep == Verb.Prep.None) {
			// No preposition -> the rest of the string is the direct object.
			param.dobjwords = remaining.ToArray();
			dobjName = string.Join(" ", param.dobjwords);
			remaining = new string[0];
		}

		// For now, the indirect object is always the rest of the phrase.
		string iobjName = null;
		if (remaining.Count() > 0) {
			param.iobjwords = remaining.ToArray();
			iobjName = string.Join(" ", param.iobjwords);
		}

		// Look for objects around the player that might match the direct and indirect objects.
		Mob dobj = ObjectMatch(dobjName, player.avatar);
		Mob iobj = ObjectMatch(iobjName, player.avatar);

		// Save the objects we found so we can verb-search.
		param.dobj = dobj;
		param.prep = p.prep;
		param.iobj = iobj;

		// Look for a matching verb.
		selectedVerb = SearchVerbsFrom(player.avatar, verb, param);
		if (selectedVerb.Count() == 0)
			selectedVerb = SearchVerbsFrom(player.avatar.location, verb, param);
		if (selectedVerb.Count() == 0 && dobj != null)
			selectedVerb = SearchVerbsFrom(dobj, verb, param);
		if (selectedVerb.Count() == 0 && iobj != null)
			selectedVerb = SearchVerbsFrom(iobj, verb, param);

		// Couldn't find one?
		if (selectedVerb.Count() != 1) {
			// Try for a "_huh" verb on the room the player is in.
			Verb huh = player.avatar.location.findVerb("_huh");
			if (huh != null) {
				param.self = player.avatar.location;
				huh.invoke(param);
				return "";
			}

			// Nothin' doin'. Just return a default error.
			return "Sorry, I don't know what that means.";
		}

		// Execute the verb.
		var v2 = selectedVerb.First();
		param.self = v2.Item1;
		v2.Item2.invoke(param);

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
			player = player,
			world = player.avatar.world
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

	static Mob ObjectMatch(string objName, Mob refObj) {
		if (string.IsNullOrEmpty(objName))
			return Mob.None;

		// Adjust any special object names.
		if ("me".EqualsI(objName))
			objName = "#{0}".FormatI(refObj.id);
		if ("here".EqualsI(objName))
			objName = "#{0}".FormatI(refObj.locationId);

		// If it's a numeric object ID, go ahead and just look it up.
		if (objName.StartsWithI("#"))
			return refObj.world.findObject(CultureFree.ParseInt(objName.Substring(1)));

		// If it's an absolute path name, look it up.
		if (objName.StartsWithI("/"))
			return refObj.world.findObject(objName);

		// Look in the normal places, otherwise.
		IEnumerable<Mob> objOptions =
			from m in refObj.contained
				.Concat(refObj.location.contained)
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

	static IEnumerable<Tuple<Mob,Verb>> SearchWildcardVerbsFrom(Mob m, string verbName,
		Verb.VerbParameters param)
	{
		param.self = m;
		foreach (var v in m.allVerbs)
			if (v.Value.item.name == verbName) {
				if (v.Value.item.matchWildcards(param).Count() > 0)
					yield return Tuple.Create(m, v.Value.item);
			}
	}
}

}
