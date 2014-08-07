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
using System.IO;
using Kayateia.Climoo.Database;
using NUnit.Framework;

/// <summary>
/// Tests for the MemoryDatabase class
/// </summary>
[TestFixture]
public class DatabaseMemoryTest
{
	[Test]
	public void connectionTest()
	{
		_dbc.connectionTest( new MemoryDatabase() );
	}

	[Test]
	public void simpleInsertAndSelectTest()
	{
		_dbc.simpleInsertAndSelectTest( new MemoryDatabase() );
	}

	// We'll go ahead and test the persisted memory database here too. It's just a variant.
	[Test]
	public void PmdConnectionTest()
	{
		string testPath = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( testPath );
		try
		{
			_dbc.connectionTest( new PersistedMemoryDatabase(), "path={0}".FormatI( testPath ) );
		}
		finally
		{
			Directory.Delete( testPath, true );
		}
	}

	[Test]
	public void PmdSimpleInsertAndSelectTest()
	{
		string testPath = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) );
		Directory.CreateDirectory( testPath );
		try
		{
			_dbc.simpleInsertAndSelectTest( new PersistedMemoryDatabase(), "path={0}".FormatI( testPath ),
				() => new PersistedMemoryDatabase() );
		}
		finally
		{
			Directory.Delete( testPath, true );
		}
	}

	DatabaseCommon _dbc = new DatabaseCommon();
}

}
