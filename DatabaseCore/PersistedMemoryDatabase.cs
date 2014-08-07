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

namespace Kayateia.Climoo.Database
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// This subclass of MemoryDatabase basically just adds file persistence to it. Updates
/// to the MemoryDatabase are committed to .NET serialization files, and that info is
/// reloaded at start time.
/// </summary>
public class PersistedMemoryDatabase : MemoryDatabase
{
	public override void setup( string connectionString, ITableInfo tableInfo )
	{
		// If we're using file backing, then the connection string will start with "file=".
		if( connectionString.StartsWith( "path=" ) )
		{
			_filepath = connectionString.Substring( "file=".Length );
			if( !Directory.Exists( _filepath ) )
				Directory.CreateDirectory( _filepath );
			loadAll();
		}
		else
			throw new ArgumentException( "Connection string does not start with 'path='" );
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
	void saveRow( string table, ulong rowId, Item row )
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
		string[] tablePaths = Directory.EnumerateDirectories( _filepath )
			.Select( p => Path.GetFileName( p ) ).ToArray();
		var tables = new Dictionary<string, Table>();
		var ser = new BinaryFormatter();
		foreach( string t in tablePaths )
		{
			string tablePath = Path.Combine( _filepath, t );
			string[] rowPaths = Directory.EnumerateFiles( tablePath )
				.Select( p => Path.GetFileName( p ) ).ToArray();
			var table = new Table();
			tables[t] = table;
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

		overwriteContents( tables );
	}

	protected override void insertedRow( string table, ulong rowId, MemoryDatabase.Item row )
	{
		saveRow( table, rowId, row );
	}

	protected override void updatedRow( string table, ulong rowId, MemoryDatabase.Item row )
	{
		saveRow( table, rowId, row );
	}

	protected override void deletedRow( string table, ulong rowId )
	{
		deleteSavedRow( table, rowId );
	}

	string _filepath = null;
}

}
