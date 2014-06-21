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
	/// Connect to the database. The connection string is provider specific.
	/// </summary>
	void connect(string connectionString);

	/// <summary>
	/// Simplistic select interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="constraints">
	/// A list of constraints to check against. Each will become a clause in a
	/// "where X=Y and A=B" constraint.
	/// </param>
	/// <returns>A list of matching objects, keyed by database ID.</returns>
	IDictionary<int, IDictionary<string, object>> select(string table, IDictionary<string, object> constraints);

	/// <summary>
	/// Simplistic update interface.
	/// </summary>
	/// <param name="table">The name of the table in question.</param>
	/// <param name="itemId">The ID of the item to update. It must exist.</param>
	/// <param name="values">Values to be updated.</param>
	void update(string table, int itemId, IDictionary<string, object> values);

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

}
