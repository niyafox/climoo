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
/// Implements a simple "matching tree", with a candy treat at the leaves.
/// This is more or less a form of Markov chains.
/// </summary>
/// <remarks>
/// The top level of a match tree should be neither a word nor value leaf node.
/// </remarks>
public class MatchTree<Word, Value>
	where Word:class
	where Value:class
{
	public MatchTree() { }

	/// <summary>
	/// The word that this node represents, or null.
	/// </summary>
	public Word word {
		get { return _word; }
		set {
			_word = value;
		}
	}
	Word _word;

	/// <summary>
	/// The leaf value this node represents, or null.
	/// </summary>
	public Value leaf { get; set; }

	/// <summary>
	/// Bundled up "match". This includes both the terminal value and the "path".
	/// </summary>
	public struct Match {
		public Value match;
		public IEnumerable<Word> path;
	}

	/// <summary>
	/// Returns a list of potential "nexts" from the available choices.
	/// </summary>
	public IEnumerable<MatchTree<Word, Value>> findNexts(Word word) {
		return
			from mt in _nexts
			where word.Equals(mt.word) || mt.leaf != null
			select mt;
	}

	/// <summary>
	/// Searches this tree for potential matches for a full string.
	/// </summary>
	/// <remarks>
	/// This is recursive. Presumably a previous search has already peeled
	/// words off, or we are at the root node.
	/// </remarks>
	public IEnumerable<Match> findMatches(IEnumerable<Word> words) {
		return findMatches(words, new Stack<MatchTree<Word, Value>>());
	}

	IEnumerable<Match> findMatches(IEnumerable<Word> words, Stack<MatchTree<Word, Value>> stack) {
		List<Match> rv = new List<Match>();
		if (this.leaf != null)
			rv.Add(new Match() {
				match = this.leaf,
				path = (from m in stack where m.word != null select m.word).ToArray()
			});

		if (words.Any()) {
			var nexts = findNexts(words.First());
			var rest = words.Skip(1);

			try {
				stack.Push(this);
				foreach (var n in nexts)
					rv.AddRange(n.findMatches(rest, stack));
			} finally {
				stack.Pop();
			}
		}

		return rv;
	}

	/// <summary>
	/// Add a full sentence into the tree.
	/// </summary>
	/// <remarks>
	/// This is recursive. Presumably a previous add has already peeled
	/// words off, or we are at the root node.
	/// </remarks>
	public void addMatch(IEnumerable<Word> words, Value result) {
		if (!words.Any()) {
			_nexts.Add(new MatchTree<Word, Value>() {
				leaf = result
			});
		} else {
			// Is there an existing path for this word?
			Word w = words.First();
			var cur =
				from n in _nexts
				where w.Equals(n.word)
				select n;
			System.Diagnostics.Debug.Assert(cur.Count() == 0 || cur.Count() == 1);

			// If yes, use it. Otherwise make a new one.
			MatchTree<Word, Value> next;
			if (cur.Count() == 1) {
				next = cur.First();
			} else {
				next = new MatchTree<Word, Value>() {
					word = words.First()
				};
				_nexts.Add(next);
			}

			next.addMatch(words.Skip(1), result);
		}
	}

	// A list of further choices.
	List<MatchTree<Word, Value>> _nexts {
		get {
			if (_nexts_backing == null)
				_nexts_backing = new List<MatchTree<Word,Value>>();
			return _nexts_backing;
		}
	}
	List<MatchTree<Word, Value>> _nexts_backing;
}

}
