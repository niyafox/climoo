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
	using System.Runtime.Serialization;

[TestFixture]
public class Serialization
{
	[Test]
	public void Json()
	{
		var tc = new TestClass()
		{
			foo = "foo",
			bar = 5,
			too = new TestClass2()
			{
				strings = new string[] { "foo", "bar", "baz" },
				ints = new int[] { 1, 2, 3, 4, 5 }
			}
		};

		string json = JsonPersistence.Serialize( tc );
		var back = JsonPersistence.Deserialize<TestClass>( json );
		Assert.AreEqual( tc.foo, back.foo );
		Assert.AreEqual( tc.bar, back.bar );
		Assert.AreEqual( tc.too.strings, back.too.strings );
		Assert.AreEqual( tc.too.ints, back.too.ints );
	}

	[DataContract]
	class TestClass
	{
		[DataMember]
		public string foo;
		[DataMember]
		public int bar;
		[DataMember]
		public TestClass2 too;
	}
	[DataContract]
	class TestClass2
	{
		[DataMember]
		public string[] strings;
		[DataMember]
		public int[] ints;
	}
}

}
