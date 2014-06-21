namespace Kayateia.Climoo.Database {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Implements IDatabase with an in-memory structure.
/// </summary>
public class MemoryDatabase : IDatabase {
	class Table {
		public Dictionary<int, Item> rows = new Dictionary<int,Item>();
		public int highId = 0;
	}
	class Item {
		public Dictionary<string, object> values = new Dictionary<string,object>();
	}

	Dictionary<string, Table> _tables = new Dictionary<string,Table>();
	object _lock = new object();

	public void connect(string connectionString) {
		// Always succeeds.
	}

	public IDictionary<int, IDictionary<string, object>> select(string table, IDictionary<string, object> constraints) {
		var results = new Dictionary<int, IDictionary<string, object>>();
		lock (_lock) {
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();
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

	bool match(Item i, IDictionary<string, object> constraints) {
		return constraints.All(
			c => i.values.ContainsKey(c.Key) && i.values[c.Key].Equals(c.Value)
		);
	}

	public void update(string table, int itemId, IDictionary<string, object> values) {
		lock (_lock) {
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();
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
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();
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
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();
			if (_tables[table].rows.ContainsKey(itemId))
				_tables[table].rows.Remove(itemId);
		}
	}
}

}
