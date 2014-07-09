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
using Kayateia.Climoo.Database;

[TestFixture]
public class CoreDatabaseTest
{
	[Test]
	public void insertAndSelectTest()
	{
		using( var token = _db.token() )
		using( _db.transaction( token ) )
		{
			ulong id = _db.insert( token, _testData );
			var selected = _db.select( token,
				new DBTest()
				{
					id = id
				},
				new string[] { "id" }
			);

			Assert.AreEqual( 1, selected.Count() );
			_testData.id = id;
			assertAreEqual( _testData, selected.First() );
		}
	}

	void assertAreEqual( TableRow row1, TableRow row2 )
	{
		var row1c = TableRow.GetColumns( row1.GetType() );
		var row2c = TableRow.GetColumns( row2.GetType() );
		Assert.AreEqual( row1c, row2c );

		foreach( string col in row1c )
		{
			var row1v = row1.GetColumnValue( col );
			var row2v = row2.GetColumnValue( col );
			Assert.AreEqual( row1v, row2v );
		}
	}

	DBTest _testData = new DBTest()
	{
		str = "this is a string",
		longer = String.Join( "this is a really long string which is fantastic", new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" } ),
		num = 10,
		datacol = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
		boolean = true,
		time = new DateTimeOffset( 2014, 6, 10, 5, 2, 30, new TimeSpan() )
	};

	CoreDatabase _db = new CoreDatabase( new MemoryDatabase() );
}

}
