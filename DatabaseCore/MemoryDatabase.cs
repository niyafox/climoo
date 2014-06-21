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

namespace Kayateia.Climoo.Database {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Implements IDatabase with an in-memory structure. This is mostly suitable for
/// testing because it is easy to use without any sort of SQL connection layer.
/// </summary>
public class MemoryDatabase : IDatabase {
	// Basic structure -- rows of key/value pairs.
	class Table {
		public Dictionary<int, Item> rows = new Dictionary<int,Item>();
		public int highId = 0;
	}
	class Item {
		public Dictionary<string, object> values = new Dictionary<string,object>();
	}

	// All of our "tables".
	Dictionary<string, Table> _tables = new Dictionary<string,Table>();
	object _lock = new object();

	public void connect(string connectionString) {
		// Always succeeds. We're not connecting to anything.
	}

	public IDictionary<int, IDictionary<string, object>> select(string table, IDictionary<string, object> constraints) {
		var results = new Dictionary<int, IDictionary<string, object>>();
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// Search through the rows to look for ones that match the given key/value pairs.
			foreach (var i in _tables[table].rows) {
				if (match(i.Value, constraints)) {
					var newResults = new Dictionary<string, object>();
					results[i.Key] = newResults;
					foreach (var pair in i.Value.values)
						newResults[pair.Key] = pair.Value;
				}
			}
		}

		return results;
	}

	// Returns true if the row matches the key/value pairs.
	bool match(Item i, IDictionary<string, object> constraints) {
		return constraints.All(
			c => i.values.ContainsKey(c.Key) && i.values[c.Key].Equals(c.Value)
		);
	}

	public void update(string table, int itemId, IDictionary<string, object> values) {
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// Look for the specified row. If we have it, update the key/value pairs.
			Item row;
			if (_tables[table].rows.TryGetValue(itemId, out row)) {
				foreach (var pair in values)
					row.values[pair.Key] = pair.Value;
			}
		}
	}

	public int insert(string table, IDictionary<string, object> values) {
		int id;
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// Add a new row with the next row ID, and fill its key/value pairs.
			Item row = new Item();
			id = ++_tables[table].highId;
			_tables[table].rows[id] = row;
			row.values["id"] = id;
			foreach (var pair in values)
				row.values[pair.Key] = pair.Value;
		}
		return id;
	}

	public void delete(string table, int itemId) {
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// If the row is in there, delete it.
			if (_tables[table].rows.ContainsKey(itemId))
				_tables[table].rows.Remove(itemId);
		}
	}
}

}
