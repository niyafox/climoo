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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

/// <summary>
/// A fully MIME-typed attribute blob. All attributes should either be strings, or
/// be objects of this type. Anything that's not a string should have a MIME type
/// associated with it and be serializable.
/// </summary>
public class TypedAttribute {
	/// <summary>
	/// Deserializes a database form into a new TypedAttribute.
	/// </summary>
	/// <remarks>
	/// Throws ArgumentException if we can't do it.
	/// </remarks>
	static public TypedAttribute FromSerialized( AttributeSerialized ser )
	{
		if( ser.mimetype == MimeNull )
			return new TypedAttribute() { contents = null };

		foreach( var handler in s_builders )
		{
			var val = handler.deserialize( ser );
			if( val != null )
				return val;
		}

		// Shouldn't actually happen.
		return null;
	}

	/// <summary>
	/// Serializes this object into database form.
	/// </summary>
	/// <remarks>
	/// Throws ArgumentException if we can't do it.
	/// </remarks>
	public AttributeSerialized serialize()
	{
		if( this.contents == null )
		{
			return new AttributeSerialized()
			{
				mimetype = MimeNull
			};
		}

		foreach( var handler in s_builders )
		{
			var val = handler.serialize( this );
			if( val != null )
				return val;
		}

		// Shouldn't actually happen.
		return null;
	}

	// Some useful MIME type strings (some invented)
	public const string MimeString = "text/plain";
	public const string MimeBinary = "application/octet-stream";
	public const string MimeClrPrefix = "clr/";
	public const string MimeImagePrefix = "image/";
	public const string MimeNull = "clr/null";

	/// <summary>
	/// Gets the full mime type for a CLR type.
	/// </summary>
	/// <param name="t">A type, or null for the null type</param>
	static public string MimeClr(Type t) {
		if (t == null)
			return "{0}null".FormatI(MimeClrPrefix);
		else
			return "{0}{1}".FormatI( MimeClrPrefix, JsonPersistence.MapTypeToSafeName( t ) );
	}

	/// <summary>
	/// Converts a CLR mime type into a CLR short type name.
	/// </summary>
	static public string MimeClrToTypename( string mime )
	{
		return mime.Substring( MimeClrPrefix.Length );
	}

	/// <summary>
	/// Get/set the contents of this attribute. The mime type will also be set
	/// unless the value is byte[] (in which case we expect the user to set it).
	/// </summary>
	public object contents {
		get { return _contents; }
		set {
			_contents = value;
			if (_contents is string || _contents is StringI)
			{
				_mimetype = MimeString;

				// Normalize on strings here.
				if( _contents is StringI )
					_contents = (string)((StringI)_contents);
			}
			/*else if (_contents is byte[])
			{
				// We let the user set this so we don't trample on an object initializer.
			} */
			else if (_contents is TypedAttribute) {
				// Big ol' error.
				throw new ArgumentException("Can't set a TypedAttribute as a TypedAttribute");
			}
			else if (_contents == null )
			{
				_mimetype = MimeNull;
			}
			else {
				_mimetype = MimeClr(_contents != null ? _contents.GetType() : null);

				// Serialized types must have a DataContract or Serializable attribute. We make a
				// minimal effort to check for arrays here too.
				if( !IsSerializable( _contents ) )
					throw new ArgumentException( "Attribute value is not serializable." );
			}
		}
	}

	static public bool IsSerializable( object o )
	{
		// Check for the surface type.
		Type t = o.GetType();

		// This should cover basic types.
		if( t.IsPrimitive || o is string )
			return true;

		// Is this an array?
		if( t.IsArray )
		{
			Array arr = (Array)o;
			foreach( object i in arr )
			{
				if( !IsSerializable( i ) )
					return false;
			}
		}
		else
		{
			if( t.GetCustomAttributes( typeof( SerializableAttribute ), true ).Length == 0 &&
					t.GetCustomAttributes( typeof( DataContractAttribute ), true ).Length == 0 )
				return false;
		}

		return true;
	}

	/// <summary>
	/// Get/set the mime type of this attribute. Set only works for byte[] objects.
	/// </summary>
	public string mimetype
	{
		get
		{
			return _mimetype;
		}
		set
		{
			_mimetype = value;
		}
	}
	object _contents;
	string _mimetype;

	/// <summary>
	/// Get a strongly-typed value out of this attribute.
	/// </summary>
	/// <remarks>
	/// This requires the value to already be the named .NET type. Check first!
	/// </remarks>
	public T getContents<T>() {
		return (T)this.contents;
	}

	/// <summary>
	/// Returns contents as if we were a string value.
	/// </summary>
	public string str { get { return getContents<string>(); } }

	/// <summary>
	/// Returns true if we are a string.
	/// </summary>
	public bool isString { get { return _mimetype.EqualsI(MimeString); } }

	/// <summary>
	/// Returns true if we are an image.
	/// </summary>
	public bool isImage { get { return _mimetype.StartsWithI(MimeImagePrefix); } }

	/// <summary>
	/// Returns true if we are null.
	/// </summary>
	public bool isNull { get { return _mimetype.EqualsI(MimeNull); } }

	/// <summary>
	/// Gets a safe-to-display-as-text string for our value.
	/// </summary>
	public string display {
		get {
			if (this.isString)
				return "\"{0}\"".FormatI(this.str);
			else if( this.mimetype.StartsWithI("clr/") )
				return ClrToDisplay( this.contents );
			else
				return "<binary data>";
		}
	}

	static string ClrToDisplay( object o )
	{
		Type t = o.GetType();
		if( t.IsArray )
		{
			Array a = (Array)o;
			return "[{0}]".FormatI( String.Join( ",", a.OfType<object>().Select( obj => ClrToDisplay( obj ) ) ) );
		}

		// Let the type do its own string conversion and return it bare. It'll make nice JSON-esque lists.
		return o.ToStringI();
	}

	/// <summary>
	/// Converts any (supported) attribute value into a boxed, well-typed value that can be persisted.
	/// </summary>
	static public TypedAttribute FromValue(object o) {
		if (o is TypedAttribute)
			return (TypedAttribute)o;
		else
			return new TypedAttribute() {
				contents = o
			};
	}

	/// <summary>
	/// Converts any (supported) attribute value into a boxed, well-typed value that can be persisted,
	/// and sets the mime type to the specific value.
	/// </summary>
	static public TypedAttribute FromValue( object o, string mimetype )
	{
		var rv = FromValue( o );
		rv.mimetype = mimetype;
		return rv;
	}

	/// <summary>
	/// Create a typed attribute from a null value.
	/// </summary>
	static public TypedAttribute FromNull()
	{
		return new TypedAttribute() { contents = null };
	}


	static Dictionary<StringI, string> ExtensionMap = new Dictionary<StringI,string> {
		{ "jpg", "image/jpeg" },
		{ "gif", "image/gif" },
		{ "png", "image/png" }
	};

	/// <summary>
	/// Create a typed attribute from a binary file on disk.
	/// </summary>
	/// <remarks>
	/// We'll take a guess at the mime type, but one can be provided to help out.
	/// </remarks>
	static public TypedAttribute LoadFromFile(string path, string mimetype = null) {
		byte[] contents;
		using (FileStream fs = File.OpenRead(path)) {
			int size = (int)fs.Length;
			contents = new byte[size];
			fs.Read(contents, 0, size);
		}

		var rv = new TypedAttribute();
		rv.contents = contents;

		if (mimetype != null)
			rv.mimetype = mimetype;
		else {
			string ext = Path.GetExtension(path);
			if (ext.StartsWith(".")) ext = ext.Substring(1);
			if (ExtensionMap.ContainsKey(ext))
				rv.mimetype = ExtensionMap[ext];
			else
				rv.mimetype = MimeBinary;
		}

		return rv;
	}

	static TypedAttribute()
	{
		s_builders = new AttributeBuilder[]
		{
			new OldStyleBinaryBuilder(),
			new BinaryArrayBuilder(),
			new StringAttributeBuilder(),
			new ClrTypeBuilder(),
			new FailBuilder()
		};
	}

	static AttributeBuilder[] s_builders;
}

/// <summary>
/// Represents the serialized form of an attribute. This is what will be passed to
/// and from the database code.
/// </summary>
public class AttributeSerialized
{
	public string mimetype;
	public string strvalue;
	public byte[] binvalue;
}

/// <summary>
/// Handles building and breaking down attributes of a given type.
/// </summary>
public abstract class AttributeBuilder
{
	public abstract AttributeSerialized serialize( TypedAttribute t );
	public abstract TypedAttribute deserialize( AttributeSerialized serialized );
}

/// <summary>
/// Handles loading old-style binary attributes. These are deprecated and will be converted.
/// </summary>
public class OldStyleBinaryBuilder : AttributeBuilder
{
	public override AttributeSerialized serialize( TypedAttribute t )
	{
		// We never serialize into this format.
		return null;
	}

	const string MimeMob = "moo/objectref";
	const string MimeMobs = "moo/objectrefs";

	public override TypedAttribute deserialize( AttributeSerialized serialized )
	{
		if( serialized.strvalue != null || serialized.binvalue == null )
			return null;

		// We go off mime types here. Most of them are deprecated now.
		if( serialized.mimetype.EqualsI( TypedAttribute.MimeString ) )
			return TypedAttribute.FromValue( Encoding.UTF8.GetString( serialized.binvalue ) );
		if( !serialized.mimetype.EqualsI( MimeMob ) && !serialized.mimetype.EqualsI( MimeMobs )
			&& !serialized.mimetype.StartsWithI( TypedAttribute. MimeClrPrefix ) )
		{
			// We don't handle these.
			return null;
		}

		// Everything else is either mob/objectref or clr/...
		var ser = new BinaryFormatter();

		var stream = new MemoryStream( serialized.binvalue );
		var obj = ser.Deserialize( stream );

		if( serialized.mimetype == MimeMob )
			return TypedAttribute.FromValue( new Mob.Ref( (int)obj ) );
		else if( serialized.mimetype == MimeMobs )
			return TypedAttribute.FromValue( ((int[])obj).Select( m => (object)(new Mob.Ref( m )) ).ToArray() );
		else
			return TypedAttribute.FromValue( obj );
	}
}

/// <summary>
/// Handles plain binary arrays. Note that the mime type may need to be adjusted depending
/// on what's in the array (image, etc).
/// </summary>
public class BinaryArrayBuilder : AttributeBuilder
{
	public override AttributeSerialized serialize( TypedAttribute t )
	{
		if( t.contents.GetType() != typeof( byte[] ) )
			return null;

		return new AttributeSerialized()
		{
			mimetype = t.mimetype,
			binvalue = (byte[])t.contents
		};
	}

	public override TypedAttribute deserialize( AttributeSerialized serialized )
	{
		// We handle everything that's in binary form, since CLR types are JSON'd now.
		if( serialized.binvalue == null || serialized.strvalue != null )
			return null;

		return TypedAttribute.FromValue( serialized.binvalue, serialized.mimetype );
	}
}

/// <summary>
/// Handles plain old strings.
/// </summary>
public class StringAttributeBuilder : AttributeBuilder
{
	public override AttributeSerialized serialize( TypedAttribute t )
	{
		object v = t.contents;
		if( !(v is StringI) && !(v is string) )
			return null;

		return new AttributeSerialized()
		{
			strvalue = (v is StringI) ? ((string)((StringI)v)) : (string)v,
			mimetype = TypedAttribute.MimeString
		};
	}

	public override TypedAttribute deserialize( AttributeSerialized serialized )
	{
		if( serialized.mimetype == TypedAttribute.MimeString )
			return TypedAttribute.FromValue( serialized.strvalue, TypedAttribute.MimeString );
		else
			return null;
	}
}

/// <summary>
/// Handles arbitrary CLR types (including mob refs). This excludes byte[].
/// </summary>
public class ClrTypeBuilder : AttributeBuilder
{
	public override AttributeSerialized serialize( TypedAttribute t )
	{
		object v = t.contents;
		if( v is byte[] )
			return null;

		return new AttributeSerialized()
		{
			strvalue = JsonPersistence.Serialize( v ),
			mimetype = TypedAttribute.MimeClr( v.GetType() )
		};
	}

	public override TypedAttribute deserialize( AttributeSerialized serialized )
	{
		if( serialized.binvalue != null || serialized.strvalue == null || !serialized.mimetype.StartsWithI( TypedAttribute.MimeClrPrefix ) )
			return null;

		Type t = JsonPersistence.MapSafeNameToType( TypedAttribute.MimeClrToTypename( serialized.mimetype ) );
		if( t == null )
			return null;

		return new TypedAttribute()
		{
			contents = JsonPersistence.Deserialize( t, serialized.strvalue ),
			mimetype = serialized.mimetype
		};
	}
}

/// <summary>
/// This sits at the bottom of the chain. If nothing else can deal with it, it's a bad value.
/// </summary>
public class FailBuilder : AttributeBuilder
{
	public override AttributeSerialized serialize( TypedAttribute t )
	{
		throw new ArgumentException( "Can't serialize attribute value." );
	}

	public override TypedAttribute deserialize( AttributeSerialized serialized )
	{
		throw new ArgumentException( "Can't deserialize attribute value." );
	}
}

}
