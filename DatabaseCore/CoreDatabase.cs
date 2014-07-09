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
using System.Linq;
using System.Text;

/// <summary>
/// Implements the schema-based, type-safe database layer on top of IDatabase.
/// </summary>
/// <remarks>
/// Most of the calls below are analogous to the ones on IDatabase, so I have not
/// duplicated the comments (inviting entropy).
/// </remarks>
public class CoreDatabase
{
	public CoreDatabase( IDatabase db )
	{
		_db = db;
	}

	public DatabaseToken token()
	{
		return _db.token();
	}

	public IEnumerable<TRow> select<TRow>( DatabaseToken token, TRow selector, IEnumerable<string> constraintColumns )
		where TRow : TableRow, new()
	{
		var constraints = new Dictionary<string, object>();
		if( constraintColumns != null )
			foreach( string col in constraintColumns )
				constraints[TableRow.GetColumnDBName( typeof( TRow ), col )] = selector.GetColumnValue( col );

		var results = _db.select( token, TableRow.GetTableName( typeof( TRow ) ), constraints );

		var rv = new List<TRow>();
		foreach( var rowkvp in results )
		{
			TRow row = new TRow();
			foreach( string col in TableRow.GetColumns( typeof( TRow ) ) )
				row.SetColumnValue( col, rowkvp.Value[TableRow.GetColumnDBName( typeof( TRow ), col )] );
			rv.Add( row );
		}

		return rv;
	}

	public void update<TRow>( DatabaseToken token, TRow row, IEnumerable<string> columnsToUpdate )
		where TRow : TableRow
	{
		var values = new Dictionary<string, object>();
		foreach( string col in columnsToUpdate )
			values[TableRow.GetColumnDBName( typeof( TRow ), col )] = row.GetColumnValue( col );

		_db.update( token, TableRow.GetTableName( typeof( TRow ) ), row.GetPK(), values );
	}

	public ulong insert<TRow>( DatabaseToken token, TRow row )
		where TRow : TableRow
	{
		var values = new Dictionary<string, object>();
		string pkcol = TableRow.GetPKName( typeof( TRow ) );
		foreach( string col in TableRow.GetColumns( typeof( TRow ) ) )
		{
			if( !col.Equals( pkcol, StringComparison.OrdinalIgnoreCase ) )
				values[TableRow.GetColumnDBName( typeof( TRow ), col )] = row.GetColumnValue( col );
		}

		ulong pk = _db.insert( token, TableRow.GetTableName( typeof( TRow ) ), values );
		row.SetPK( pk );
		return pk;
	}

	public void delete<TRow>( DatabaseToken token, ulong id )
		where TRow : TableRow
	{
		_db.delete( token, TableRow.GetTableName( typeof( TRow ) ), id );
	}

	public void delete<TRow>( DatabaseToken token, TRow row, IEnumerable<string> constraintColumns )
		where TRow : TableRow
	{
		var values = new Dictionary<string, object>();
		foreach( string col in constraintColumns )
			values[TableRow.GetColumnDBName( typeof( TRow ), col )] = row.GetColumnValue( col );

		_db.delete( token, TableRow.GetTableName( typeof( TRow ) ), values );
	}

	public DatabaseTransaction transaction( DatabaseToken token )
	{
		return _db.transaction( token );
	}

	IDatabase _db;
}

}
