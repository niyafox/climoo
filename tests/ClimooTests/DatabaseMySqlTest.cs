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

namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
	using System.IO;
using Kayateia.Climoo.Database;
	using Kayateia.Climoo.Database.Xml;

/// <summary>
/// Tests for the DatabaseMySql assembly
/// </summary>
/// <remarks>
/// This requires the Test table to be installed in the database, which it ought to
/// be if you used the mysql.sql file to create it. You should NOT point this at your
/// production database.
/// </remarks>
[TestFixture]
public class DatabaseMySqlTest
{
	[Test]
	public void connectionTest()
	{
		try
		{
			setupConfig();
			_db.connect( _cfg.ConnectionString, _cfg.DatabaseBinaryPath, _ti );
		}
		finally
		{
			shutdown();
		}
	}

	[Test]
	public void simpleInsertAndSelectTest()
	{
		try
		{
			setupConfig();
			_db.connect( _cfg.ConnectionString, _cfg.DatabaseBinaryPath, _ti );
			using( _db.transaction() )
			{
				int id = _db.insert( "test", _testData );
				var selected = _db.select( "test",
					new Dictionary<string, object>()
					{
						{ "id", id }
					}
				);

				Assert.AreEqual( 1, selected.Count );
				_testData["id"] = id;
				Assert.AreEqual( _testData, selected.Values.First() );
			}
		}
		finally
		{
			shutdown();
		}
	}

	IDictionary<string, object> _testData = new Dictionary<string, object>()
	{
		{ "str", "this is a string" },
		{ "longer", String.Join( "this is a really long string which is fantastic", new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" } ) },
		{ "num", 10 },
		{ "datacol", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } },
		{ "bool", true },
		{ "time", new DateTimeOffset( 2014, 6, 10, 5, 2, 30, new TimeSpan() ) }
	};

	void setupConfig()
	{
		// This should be $climooRoot/tests.
		string nunitPath = TestContext.CurrentContext.WorkDirectory;

		// We want to find the impexporter config file.
		string configPath = Path.Combine( nunitPath, "..", "..", "..", "config.xml" );
		_cfg = XmlPersistence.Load<ImpExporterConfig>( configPath );

		// Create the database itself.
		_db = new MySqlDatabase();

		_ti = new TableInfo();
	}

	void shutdown()
	{
		if( _db != null )
		{
			((IDisposable)_db).Dispose();
			_db = null;
			_cfg = null;
		}
	}

	ImpExporterConfig _cfg;
	IDatabase _db;
	TableInfo _ti;
}

}
