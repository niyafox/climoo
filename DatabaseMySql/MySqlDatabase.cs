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
	public void setup( string connectionString, string fileBase, ITableInfo tableInfo )
	{
		_connString = connectionString;
		_fileBase = fileBase;
		_tableInfo = tableInfo;
		try
		{
			// Also make the blob directory if we need it.
			if( !Directory.Exists( _fileBase ) )
				Directory.CreateDirectory( _fileBase );
		}
		catch ( Exception ex )
		{
			Log.Error( "Couldn't setup blob dir for MySQL: {0}", ex );
		}
	}

	public DatabaseToken token()
	{
		try
		{
			MySqlConnection conn = new MySqlConnection( _connString );
			conn.Open();
			return new MySqlToken( conn );
		}
		catch( Exception ex )
		{
			Log.Error( "Couldn't connect to MySQL: {0}", ex );
			throw;
		}
	}

	public void Dispose()
	{
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
				string fn = Guid.NewGuid().ToString( "N", CultureFree.Culture );
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

	// This does the actual select work, returning the rows in raw form from the
	// database. We do this separately because it's used in deletion below.
	IDictionary<ulong, IDictionary<string, object>> selectRaw( MySqlConnection conn, string table, IDictionary<string, object> constraints )
	{
		// Build a parameterized SQL query.
		string sql =  CultureFree.Format( "select * from {0}", table );
		if( constraints.Count() > 0 )
			sql += makeWhereClause( constraints );

		MySqlCommand cmd = new MySqlCommand( sql, conn );
		insertQueryParameters( cmd, table, constraints );

		var rv = new Dictionary<ulong, IDictionary<string, object>>();
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

				ulong pkId = (ulong)rdr[pkName];
				var row = new Dictionary<string, object>();
				foreach( string col in columnNames )
					row[col] = rdr[col];

				rv[pkId] = row;
			}
		}

		return rv;
	}

	// Peforms a transformation pass on the raw selected data to take binary columns
	// and other gremlins into account.
	public IDictionary<ulong, IDictionary<string, object>> select( DatabaseToken token, string table, IDictionary<string, object> constraints )
	{
		var raw = selectRaw( MySqlToken.Crack( token ), table, constraints );
		var rv = new Dictionary<ulong, IDictionary<string, object>>();

		foreach( var inrow in raw )
		{
			var outrow = new Dictionary<string, object>();
			foreach( var incol in inrow.Value )
			{
				object val = incol.Value;

				// DBNulls become nulls.
				if( val is System.DBNull )
					val = null;

				// Convert DateTime to DateTimeOffset; assume UTC.
				if( val is System.DateTime )
					val = new DateTimeOffset( (System.DateTime)val, new TimeSpan() );

				// Is it a boolean type? We store these in MySQL as integers.
				Type colType = _tableInfo.getColumnType( table, incol.Key );
				if( colType == typeof( bool ) )
					val = Convert.ToBoolean( val );

				// Read back binary data if needed.
				if( _tableInfo.isBinary( table, incol.Key ) && val != null )
				{
					string path = Path.Combine( _fileBase, (string)val );
					using( FileStream f = File.OpenRead( path ) )
					{
						byte[] bval = new byte[f.Length];
						f.Read( bval, 0, (int)f.Length );
						val = bval;
					}
				}

				outrow[incol.Key] = val;
			}
			rv[inrow.Key] = outrow;
		}

		return rv;
	}

	public void update( DatabaseToken token, string table, ulong itemId, IDictionary<string, object> values )
	{
		// Build a parameterized SQL query.
		string idName = _tableInfo.getIdColumn( table );
		string sql =  CultureFree.Format( "update {0} set {1} where {2}=@{2}",
			table,
			String.Join( ",", (from c in values.Keys select CultureFree.Format( "{0}=@{0}", c )).ToArray() ),
			idName
		);

		// Add in the updated values, plus the ID for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, MySqlToken.Crack( token ) );
		insertQueryParameters( cmd, table, values, new string[] { idName } );
		cmd.Parameters.AddWithValue( "@" + idName, itemId );

		// Do the update.
		cmd.ExecuteNonQuery();
	}

	public ulong insert( DatabaseToken token, string table, IDictionary<string, object> values )
	{
		// Build a parameterized SQL query.
		string sql =  CultureFree.Format( "insert into {0} ({1}) values ({2})",
			table,
			String.Join( ",", (from c in values.Keys select c).ToArray() ),
			String.Join( ",", (from c in values.Keys select "@" + c).ToArray() )
		);

		MySqlCommand cmd = new MySqlCommand( sql, MySqlToken.Crack( token ) );
		insertQueryParameters( cmd, table, values );

		// Do the insert and return the PK ID.
		cmd.ExecuteNonQuery();
		return (ulong)cmd.LastInsertedId;
	}

	void deleteBinaryColumns( string table, IEnumerable<string> binaryCols, IDictionary<ulong, IDictionary<string, object>> rows )
	{
		// Delete each file.
		foreach( var row in rows )
			foreach( string c in binaryCols )
			{
				if( row.Value[c] is DBNull )
					continue;

				string fn = Path.Combine( _fileBase, (string)row.Value[c] );
				if( File.Exists( fn ) )
					File.Delete( fn );
			}
	}

	public void delete( DatabaseToken token, string table, ulong itemId )
	{
		// Are there any binary columns in this table? If so, we need to pay attention
		// to eliminating the files from the file system too.
		IEnumerable<string> binaryCols = findBinaryColumns( table );
		if( binaryCols.Any() )
		{
			// Find the row we're going to delete.
			var results = selectRaw( MySqlToken.Crack( token ), table,
				new Dictionary<string, object>()
				{
					{ _tableInfo.getIdColumn( table ), itemId }
				}
			);
			if( !results.Any() )
				return;

			deleteBinaryColumns( table, binaryCols, results );
		}

		// Build a parameterized SQL query.
		string idName = _tableInfo.getIdColumn( table );
		string sql =  CultureFree.Format( "delete from {0} where {1}=@{1}", table, idName );

		// Add in the PK for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, MySqlToken.Crack( token ) );
		cmd.Parameters.AddWithValue( "@" + idName, itemId );

		// Do the delete.
		cmd.ExecuteNonQuery();
	}

	IEnumerable<string> findBinaryColumns( string table )
	{
		var allCols = _tableInfo.getAllColumns( table );
		return allCols.Where( c => _tableInfo.isBinary( table, c ) );
	}

	string makeWhereClause( IDictionary<string, object> constraints )
	{
		return CultureFree.Format( " where {0}",
			String.Join( " and ", (from c in constraints.Keys select CultureFree.Format( "{0}=@{0}", c )).ToArray() ) );
	}

	public void delete( DatabaseToken token, string table, IDictionary<string, object> constraints )
	{
		// Are there any binary columns in this table? If so, we need to pay attention
		// to eliminating the files from the file system too.
		IEnumerable<string> binaryCols = findBinaryColumns( table );
		if( binaryCols.Any() )
		{
			// Find the row we're going to delete.
			var results = selectRaw( MySqlToken.Crack( token ), table, constraints );
			if( !results.Any() )
				return;

			deleteBinaryColumns( table, binaryCols, results );
		}

		string sql =  CultureFree.Format( "delete from {0}", table );
		if( constraints.Count() > 0 )
			sql += makeWhereClause( constraints );

		// Add in the updated values, plus the ID for the where clause.
		MySqlCommand cmd = new MySqlCommand( sql, MySqlToken.Crack( token ) );
		insertQueryParameters( cmd, table, constraints );

		// Do the delete.
		cmd.ExecuteNonQuery();
	}

	public DatabaseTransaction transaction( DatabaseToken token )
	{
		MySqlTransaction trans = MySqlToken.Crack( token ).BeginTransaction();
		return new MySqlDBTransaction( trans );
	}

	string _connString;
	string _fileBase;
	ITableInfo _tableInfo;
}

/// <summary>
/// MySQL transaction wrapper.
/// </summary>
public class MySqlDBTransaction : DatabaseTransaction
{
	public MySqlDBTransaction( MySqlTransaction trans )
	{
		_trans = trans;
		_completed = false;
	}

	public override void commit()
	{
		if( !_completed )
		{
			_trans.Commit();
			_completed = true;
		}
	}

	public override void rollback()
	{
		if( !_completed )
		{
			_trans.Rollback();
			_completed = true;
		}
	}

	bool _completed;
	MySqlTransaction _trans;
}

class MySqlToken : DatabaseToken
{
	public MySqlToken( MySqlConnection conn )
	{
		this.conn = conn;
	}

	public MySqlConnection conn
	{
		get;
		private set;
	}

	public override void close()
	{
		if( this.conn != null )
		{
			this.conn.Close();
			this.conn = null;
		}
	}

	static public MySqlConnection Crack( DatabaseToken token )
	{
		return ((MySqlToken)token).conn;
	}
}

}
