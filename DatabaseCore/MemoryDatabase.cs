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
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Implements IDatabase with an in-memory structure. This is mostly suitable for
/// testing because it is easy to use without any sort of SQL connection layer.
/// </summary>
public class MemoryDatabase : IDatabase {
	// Basic structure -- rows of key/value pairs.
	protected class Table {
		public Dictionary<ulong, Item> rows = new Dictionary<ulong,Item>();
		public ulong highId = 0;
	}

	// For some bizarre reason this must be public and not protected, for access in subclasses.
	[Serializable]
	public class Item {
		public Dictionary<string, object> values = new Dictionary<string,object>();
	}

	// All of our "tables".
	Dictionary<string, Table> _tables = new Dictionary<string,Table>();
	object _lock = new object();

	public virtual void setup( string connectionString, ITableInfo tableInfo )
	{
		// Always succeeds. We're not connecting to anything. We also don't need tableInfo.
	}

	// These can be overridden to catch events in subclasses.
	protected virtual void insertedRow( string table, ulong rowId, Item row ) { }
	protected virtual void updatedRow( string table, ulong rowId, Item row ) { }
	protected virtual void deletedRow( string table, ulong rowId ) { }

	// Allows an overwrite of our contents with something else.
	protected void overwriteContents( Dictionary<string, Table> t )
	{
		lock( _lock )
			_tables = t;
	}

	public DatabaseToken token()
	{
		return new BlankToken();
	}

	public IDictionary<ulong, IDictionary<string, object>> select( DatabaseToken token, string table, IDictionary<string, object> constraints )
	{
		var results = new Dictionary<ulong, IDictionary<string, object>>();
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

	public void update( DatabaseToken token, string table, ulong itemId, IDictionary<string, object> values )
	{
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// Look for the specified row. If we have it, update the key/value pairs.
			Item row;
			if (_tables[table].rows.TryGetValue(itemId, out row)) {
				foreach (var pair in values)
					row.values[pair.Key] = pair.Value;
				updatedRow( table, itemId, row );
			}
		}
	}

	public ulong insert( DatabaseToken token, string table, IDictionary<string, object> values )
	{
		ulong id;
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
			insertedRow( table, id, row );
		}
		return id;
	}

	public void delete( DatabaseToken token, string table, ulong itemId )
	{
		lock (_lock) {
			// Look for the matching table.
			if (!_tables.ContainsKey(table))
				_tables[table] = new Table();

			// If the row is in there, delete it.
			if (_tables[table].rows.ContainsKey(itemId))
			{
				_tables[table].rows.Remove(itemId);
				deletedRow( table, itemId );
			}
		}
	}

	public void delete( DatabaseToken token, string table, IDictionary<string, object> constraints )
	{
		lock( _lock )
		{
			// Look for the matching table.
			if( !_tables.ContainsKey( table ) )
				_tables[table] = new Table();

			// Search through the rows to look for ones that match the given key/value pairs.
			var toDelete = new List<ulong>();
			foreach( var i in _tables[table].rows )
			{
				if( match( i.Value, constraints ) )
					toDelete.Add( i.Key );
			}

			// And go back and delete them. We do this separately because you can't modify
			// a collection while you're iterating over it.
			foreach( var i in toDelete )
			{
				_tables[table].rows.Remove( i );
				deletedRow( table, i );
			}
		}
	}

	public DatabaseTransaction transaction( DatabaseToken token )
	{
		// For now, we do nothing here.
		return new BlankTransaction();
	}
}


}
