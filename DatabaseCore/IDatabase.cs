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
/// Used for abstract access to the database. This database underlies both the
/// world and administrative data for the web site (like screens, logins, etc).
/// 
/// This basically just implements a simple data store, not a true SQL
/// infrastructure or anything.
/// </summary>
public interface IDatabase {
	/// <summary>
	/// Connect to the database. The connection string is provider specific. The path is
	/// for the database provider to use if it wishes to write blobs out as files.
	/// </summary>
	/// <remarks>
	/// The tableInfo parameter may be null if it's not required for this provider.
	/// </remarks>
	void connect( string connectionString, string fileBase, ITableInfo tableInfo );

	/// <summary>
	/// Simplistic select interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="constraints">
	/// A list of constraints to check against. Each will become a clause in a
	/// "where X=Y and A=B" constraint.
	/// </param>
	/// <returns>A list of matching objects, keyed by database ID.</returns>
	IDictionary<int, IDictionary<string, object>> select( string table, IDictionary<string, object> constraints );

	/// <summary>
	/// Simplistic update interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="itemId">The ID of the item to update. It must exist.</param>
	/// <param name="values">Values to be updated.</param>
	void update( string table, int itemId, IDictionary<string, object> values );

	/// <summary>
	/// Simplistic insert interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="values">
	/// Values to be inserted. Every non-nullable column must have a value.
	/// </param>
	/// <returns>The new object's ID.</returns>
	int insert(string table, IDictionary<string, object> values);

	/// <summary>
	/// Simplistic delete interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="itemId">The item's ID.</param>
	/// <remarks>Does NOT take care of dependent items.</remarks>
	void delete(string table, int itemId);

	/// <summary>
	/// More complex delete interface with a "where" clause.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="constraints">
	/// A list of constraints to check against. Each will become a clause in a
	/// "where X=Y and A=B" constraint.
	/// </param>
	void delete( string table, IDictionary<string, object> constraints );

	/// <summary>
	/// Begins a database transaction.
	/// </summary>
	/// <returns>A transaction object that may be used to control the results.</returns>
	/// <remarks>
	/// Support for transactions is highly variable. It may be the outer transaction only,
	/// or it may be no transaction support at all. YMMV. This is supplied in case it's available.
	/// </remarks>
	DatabaseTransaction transaction();
}

/// <summary>
/// Returns information about tables, which the IDatabase provider can use to
/// be intelligent about things like primary keys and binary data.
/// </summary>
/// <remarks>
/// The underlying implementation of IDatabase can take one of these to improve
/// its ability to interact with the data, or it can ignore it. At least one of
/// these should be implemented for that purpose, though.
/// </remarks>
public interface ITableInfo
{
	/// <summary>
	/// Returns the name of the ID column for the specified table.
	/// </summary>
	string getIdColumn( string table );

	/// <summary>
	/// Returns true if the specified column is a blob/binary type.
	/// </summary>
	bool isBinary( string table, string columnName );

	/// <summary>
	/// Gets the .NET type of the specified column.
	/// </summary>
	Type getColumnType( string table, string columnName );
}

/// <summary>
/// Thrown for failed database operations.
/// </summary>
public class DatabaseException : System.Exception {
	public DatabaseException(string cause)
		: base(cause)
	{
	}

	public DatabaseException(string cause, System.Exception inner)
		: base(cause, inner)
	{
	}
}

/// <summary>
/// Database transaction wrapper. This is a using-compatible class that manages a
/// transaction for you. The default action is to roll back, so call Commit() before
/// you're finished if that's what you want.
/// </summary>
public abstract class DatabaseTransaction : IDisposable
{
	public void Dispose()
	{
		// Default to rolling back, in the case of exceptions or whatnot.
		rollback();
	}

	/// <summary>
	/// Commit the transaction.
	/// </summary>
	/// <remarks>
	/// This object will no longer be valid after this is called.
	/// </remarks>
	public abstract void commit();

	/// <summary>
	/// Roll back the transaction.
	/// </summary>
	/// <remarks>
	/// This object will no longer be valid after this is called.
	/// </remarks>
	public abstract void rollback();
}

/// <summary>
/// Stub class for things in DatabaseCore to implement this class with no innards.
/// </summary>
public class BlankTransaction : DatabaseTransaction
{
	public override void commit()
	{
	}

	public override void rollback()
	{
	}
}

}
