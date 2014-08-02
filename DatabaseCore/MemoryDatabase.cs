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
using System.Runtime.Serialization.Formatters.Binary;

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
	class Table {
		public Dictionary<ulong, Item> rows = new Dictionary<ulong,Item>();
		public ulong highId = 0;
	}
	[Serializable]
	class Item {
		public Dictionary<string, object> values = new Dictionary<string,object>();
	}

	// All of our "tables".
	Dictionary<string, Table> _tables = new Dictionary<string,Table>();
	object _lock = new object();
	string _filepath = null;

	public void setup( string connectionString, ITableInfo tableInfo )
	{
		// If we're using file backing, then the connection string will start with "file=".
		if( connectionString.StartsWith( "file=" ) )
		{
			_filepath = connectionString.Substring( "file=".Length );
			if( !Directory.Exists( _filepath ) )
				Directory.CreateDirectory( _filepath );
			loadAll();
		}
	}

	// Returns a path representing the specified row ID.
	string getRowPath( string table, ulong rowId )
	{
		return Path.Combine( _filepath, table, "{0}".FormatI( rowId ) );
	}

	// Returns a path representing the specified row ID, and creates the dirs leading to it if they don't exist.
	string getAndMakeRowPath( string table, ulong rowId )
	{
		string path = getRowPath( table, rowId );
		string dirName = Path.GetDirectoryName( path );
		if( !Directory.Exists( dirName ) )
			Directory.CreateDirectory( dirName );
		return path;
	}

	// Saves out a row if we are in file mode. Otherwise does nothing.
	void saveRow( string table, ulong rowId, object row )
	{
		if( String.IsNullOrEmpty( _filepath ) )
			return;
		string rowPath = getAndMakeRowPath( table, rowId );
		var ser = new BinaryFormatter();
		var stream = new MemoryStream();
		ser.Serialize( stream, row );
		File.WriteAllBytes( rowPath, stream.GetBuffer() );
	}

	// Deletes a saved-out row if we are in file mode. Otherwise does nothing.
	void deleteSavedRow( string table, ulong rowId )
	{
		if( String.IsNullOrEmpty( _filepath ) )
			return;
		string path = getRowPath( table, rowId );
		if( File.Exists( path ) )
			File.Delete( path );
	}

	// Loads all of the rows from disk if we have any.
	void loadAll()
	{
		string[] tables = Directory.EnumerateDirectories( _filepath )
			.Select( p => Path.GetFileName( p ) ).ToArray();
		_tables = new Dictionary<string, Table>();
		var ser = new BinaryFormatter();
		foreach( string t in tables )
		{
			string tablePath = Path.Combine( _filepath, t );
			string[] rowPaths = Directory.EnumerateFiles( tablePath )
				.Select( p => Path.GetFileName( p ) ).ToArray();
			var table = new Table();
			_tables[t] = table;
			foreach( string r in rowPaths )
			{
				string rowPath = Path.Combine( tablePath, r );
				ulong id = ulong.Parse( r );
				var stream = new MemoryStream( File.ReadAllBytes( rowPath ) );
				Item row = (Item)ser.Deserialize( stream );
				table.highId = Math.Max( table.highId, id );
				table.rows[id] = row;
			}
		}
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
				saveRow( table, itemId, row );
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
			saveRow( table, id, row );
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
				deleteSavedRow( table, itemId );
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
				deleteSavedRow( table, i );
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
