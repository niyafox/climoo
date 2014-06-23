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
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

/// <summary>
/// Implements IDatabase using the MySQL .NET connector.
/// </summary>
public class MySqlDatabase : IDatabase, IDisposable {
	public void connect(string connectionString) {
		_conn = new MySqlConnection(connectionString);
		try {
			Log.Info("Connecting to MySQL...");
			_conn.Open();
		} catch (Exception ex) {
			Log.Error("Couldn't connect to MySQL: {0}", ex);
		}
	}

	public void Dispose() {
		Log.Info("Closing connection to MySQL.");
		_conn.Close();
	}
	
	public IDictionary<int, IDictionary<string, object>> select(string table, IDictionary<string, object> constraints) {
		throw new System.NotImplementedException ();
	}

	public void update(string table, int itemId, IDictionary<string, object> values) {
		throw new System.NotImplementedException ();
	}

	public int insert(string table, IDictionary<string, object> values) {
		throw new System.NotImplementedException ();
	}

	public void delete(string table, int itemId) {
		throw new System.NotImplementedException ();
	}

	MySqlConnection _conn;
}

}
