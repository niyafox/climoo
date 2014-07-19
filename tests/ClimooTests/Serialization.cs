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
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Kayateia.Climoo.MooCore;
using NUnit.Framework;

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

	[Test]
	public void TypedAttributeTest()
	{
		// Basic strings first.
		TypedAttribute str = TypedAttribute.FromValue( "Testing 1 2 3!" );
		AttributeSerialized ser = str.serialize();
		TypedAttribute strunser = TypedAttribute.FromSerialized( ser );
		Assert.AreEqual( str.str, strunser.str );

		// Simple CLR types.
		TypedAttribute intarr = TypedAttribute.FromValue( new int[] { 1, 2, 3, 4, 5 } );
		ser = intarr.serialize();
		TypedAttribute intunser = TypedAttribute.FromSerialized( ser );
		Assert.AreEqual( intarr.getContents<int[]>(), intunser.getContents<int[]>() );

		// An image.
		byte[] imgdata = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
		TypedAttribute img = TypedAttribute.FromValue( imgdata, "image/jpeg" );
		ser = img.serialize();
		TypedAttribute imgunser = TypedAttribute.FromSerialized( ser );
		Assert.AreEqual( imgdata, imgunser.getContents<byte[]>() );

		// Deserialize old style binary data.
		int[] mobrefs = { 1, 2, 3, 4 };
		var binser = new BinaryFormatter();
		var stream = new MemoryStream();
		binser.Serialize( stream, mobrefs );
		AttributeSerialized attrser = new AttributeSerialized()
		{
			binvalue = stream.ToArray(),
			mimetype = "moo/objectrefs"
		};
		TypedAttribute mobunser = TypedAttribute.FromSerialized( attrser );
		Assert.AreEqual( mobrefs, ((object[])mobunser.contents).Select( m => ((Mob.Ref)m).id ).ToArray() );

		Assert.Catch( typeof( ArgumentException ), () =>
		{
			TypedAttribute attr = TypedAttribute.FromValue( new Undecorated() );
			attr.serialize();
		} );
		Assert.Catch( typeof( ArgumentException ), () =>
		{
			TypedAttribute attr2 = TypedAttribute.FromValue( new Undecorated[] { new Undecorated() } );
			attr2.serialize();
		} );
		Assert.Catch( typeof( ArgumentException ), () =>
		{
			TypedAttribute attr3 = TypedAttribute.FromValue(
				new Undecorated[][]
				{
					new Undecorated[]
					{
						new Undecorated()
					}
				}
			);
			attr3.serialize();
		} );
	}
	class Undecorated
	{
	}
}

}
