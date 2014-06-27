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

// NOTE: This module links with the MySQL connector under the "FOSS License Exception":
//
// http://www.mysql.com/about/legal/licensing/foss-exception/
//
// This is necessary because MySQL is GPLv2 only.

namespace Kayateia.Climoo.Database {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

/// <summary>
/// Implements IDatabase using the MySQL .NET connector.
/// </summary>
public class MySqlDatabase : IDatabase, IDisposable
{
	public void connect( string connectionString, string fileBase, ITableInfo tableInfo )
	{
		if( _conn != null )
		{
			_conn.Close();
			_conn = null;
		}

		_conn = new MySqlConnection( connectionString );
		_fileBase = fileBase;
		_tableInfo = tableInfo;
		try
		{
			Log.Info( "Connecting to MySQL..." );
			_conn.Open();

			// Also make the blob directory if we need it.
			if( !Directory.Exists( _fileBase ) )
				Directory.CreateDirectory( _fileBase );
		}
		catch ( Exception ex )
		{
			Log.Error( "Couldn't connect to MySQL: {0}", ex );
		}
	}

	public void Dispose()
	{
		Log.Info( "Closing connection to MySQL." );
		if( _conn != null )
		{
			_conn.Close();
			_conn = null;
		}
	}

	// Pulls the values out of the param dictionary in as SQL parameters.
	void insertQueryParameters( MySqlCommand cmd, string table, IDictionary<string, object> param, string[] exclude = null )
	{
		foreach( var pair in param )
		{
			if( exclude != null && exclude.Any( e => e == pair.Key ) )
				continue;

			object val = pair.Value;
			if( _tableInfo.isBinary( table, pair.Key ) && val is byte[] )
			{
				// In the case of binary data, we actually want to write it out to disk and
				// substitute a filename in the tables.
				byte[] array = (byte[])val;
				string fn = SHA1( array );
				string path = Path.Combine( _fileBase, fn );
				using( FileStream f = File.OpenWrite( path ) )
					f.Write( array, 0, array.Length );

				val = fn;
			}

			if( val is DateTimeOffset )
			{
				// We have to do this dance to get it into the right value in UTC.
				DateTime dt = ((DateTimeOffset)val).UtcDateTime;
				dt = new DateTime( dt.Ticks, DateTimeKind.Unspecified );
				val = dt;
			}

			cmd.Parameters.AddWithValue( "@" + pair.Key, val );
		}
	}
	
	public IDictionary<int, IDictionary<string, object>> select( string table, IDictionary<string, object> constraints )
	{
		// Build a parameterized SQL query.
		string sql =  CultureFree.Format( "select * from {0}", table );
		if( constraints.Count() > 0 )
			sql += makeWhereClause( constraints );

		MySqlCommand cmd = new MySqlCommand( sql, _conn );
		insertQueryParameters( cmd, table, constraints );

		var rv = new Dictionary<int, IDictionary<string, object>>();
		string pkName = _tableInfo.getIdColumn( table );
		using( MySqlDataReader rdr = cmd.ExecuteReader() )
		{
			string[] columnNames = null;
			while( rdr.Read() )
			{
				if( columnNames == null )
				{
					columnNames = new string[rdr.FieldCount];
					for( int i=0; i<rdr.FieldCount; ++i )
						columnNames[i] = rdr.GetName( i );
				}

				int pkId = (int)rdr[pkName];
				var row = new Dictionary<string, object>();
				foreach( string col in columnNames )
				{
					object val = rdr[col];

					// DBNulls become nulls.
					if( val is System.DBNull )
						val = null;

					// Convert DateTime to DateTimeOffset; assume UTC.
					if( val is System.DateTime )
						val = new DateTimeOffset( (System.DateTime)val, new TimeSpan() );

					// Read back binary data if needed.
					if( _tableInfo.isBinary( table, col ) && val != null )
					{
						string path = Path.Combine( _fileBase, (string)val );
						using( FileStream f = File.OpenRead( path ) )
						{
							byte[] bval = new byte[f.Length];
							f.Read( bval, 0, (int)f.Length );
							val = bval;
						}
					}
					row[col] = val;
				}

				rv[pkId] = row;
			}
		}

		return rv;
	}

	public void update( string table, int itemId, IDictionary<string, object> values )
	{
		// Build a parameterized SQL query.
		string idName = _tableInfo.getIdColumn( table );
		string sql =  CultureFree.Format( "update {0} set {1} where {2}=@{2}",
			table,
			String.Join( ",", (from c in values.Keys select CultureFree.Format( "{0}=@{0}", c )).ToArray() ),
			idName
		);

		// Add in the updated values, plus the ID for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, _conn );
		insertQueryParameters( cmd, table, values, new string[] { idName } );
		cmd.Parameters.AddWithValue( "@" + idName, itemId );

		// Do the update.
		cmd.ExecuteNonQuery();
	}

	// Returns a filename-suitable string hashed from the specified data.
	static string SHA1( byte[] data )
	{
	    var sha1 = new SHA1CryptoServiceProvider();
		return BitConverter.ToString( sha1.ComputeHash( data ) );
	}

	public int insert(string table, IDictionary<string, object> values)
	{
		// Build a parameterized SQL query.
		string sql =  CultureFree.Format( "insert into {0} ({1}) values ({2})",
			table,
			String.Join( ",", (from c in values.Keys select c).ToArray() ),
			String.Join( ",", (from c in values.Keys select "@" + c).ToArray() )
		);

		MySqlCommand cmd = new MySqlCommand( sql, _conn );
		insertQueryParameters( cmd, table, values );

		// Do the insert and return the PK ID.
		cmd.ExecuteNonQuery();
		return (int)cmd.LastInsertedId;
	}

	public void delete( string table, int itemId )
	{
		// Build a parameterized SQL query.
		string idName = _tableInfo.getIdColumn( table );
		string sql =  CultureFree.Format( "delete from {0} where {1}=@{1}", table, idName );

		// Add in the PK for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, _conn );
		cmd.Parameters.AddWithValue( "@" + idName, itemId );

		// Do the delete.
		cmd.ExecuteNonQuery();
	}

	string makeWhereClause( IDictionary<string, object> constraints )
	{
		return CultureFree.Format( " where {0}",
			String.Join( " and ", (from c in constraints.Keys select CultureFree.Format( "{0}=@{0}", c )).ToArray() ) );
	}

	public void delete( string table, IDictionary<string, object> constraints )
	{
		string sql =  CultureFree.Format( "delete from {0}", table );
		if( constraints.Count() > 0 )
			sql += makeWhereClause( constraints );

		// Add in the updated values, plus the ID for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, _conn );
		insertQueryParameters( cmd, table, constraints );

		// Do the delete.
		cmd.ExecuteNonQuery();
	}

	MySqlConnection _conn;
	string _fileBase;
	ITableInfo _tableInfo;
}

}
