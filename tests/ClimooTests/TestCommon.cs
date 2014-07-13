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
using System.IO;
using NUnit.Framework;

class TestCommon
{
	// Returns the path to climoo/tests/
	static public string GetTesterPath()
	{
		string nunitPath = TestContext.CurrentContext.WorkDirectory;
		return Path.Combine( nunitPath, "..", "..", ".." );
	}

	// Returns the path to the database config file.
	static public string GetDatabaseConfigPath()
	{
		return Path.Combine( GetTesterPath(), "config.xml" );
	}

	// Returns the path to the ref file for a ref/out test.
	static public string GetRefFilePath( string testName )
	{
		return Path.Combine( GetTesterPath(), "refs", testName ) + ".ref";
	}

	// Returns the path to the out file for a ref/out test.
	static public string GetOutFilePath( string testName )
	{
		return Path.Combine( GetTesterPath(), "refs", testName ) + ".out";
	}

	// Writes output from a ref/out test to the out file, and compares it to the ref file.
	static public void CompareRef( string testName, string outText )
	{
		string testOutPath = GetOutFilePath( testName );
		if( !Directory.Exists( Path.GetDirectoryName( testOutPath ) ) )
			Directory.CreateDirectory( Path.GetDirectoryName( testOutPath ) );

		File.WriteAllText( testOutPath, outText );

		string testRefPath = GetRefFilePath( testName );
		if( !File.Exists( testRefPath ) )
		{
			Assert.False( true, "Ref does not exist" );
			return;
		}
		string refText = File.ReadAllText( testRefPath );
		Assert.AreEqual( refText, outText );
	}
}

}
