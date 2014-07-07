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
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// A fully MIME-typed attribute blob. All attributes should either be strings, or
/// be objects of this type. Anything that's not a string should have a MIME type
/// associated with it and be serializable.
/// </summary>
public class TypedAttribute {
	// Some useful MIME type strings (some invented)
	public const string MimeString = "text/plain";
	public const string MimeMob = "moo/objectref";
	public const string MimeMobs = "moo/objectrefs";
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
			return "{0}{1}".FormatI(MimeClrPrefix, t.FullName);
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
				_mimetype = MimeString;
			else if (_contents is byte[]) {
				// We let the user set this so we don't trample on an object initializer.
			} else if (_contents is Mob.Ref) {
				// We store this by ID later.
				_mimetype = MimeMob;
			} else if( _contents is object[] ) {
				// If this isn't an array of primitives or Mob.Refs, it could cause some pretty gross
				// issues. I'm not sure of the best way to deal with this right now.
				object[] arr = (object[])_contents;
				if( arr.Length == 0 || arr[0] is Mob.Ref )
					_mimetype = MimeMobs;
				else
					_mimetype = MimeClr( _contents.GetType() );
			} else if (_contents is TypedAttribute) {
				// Big ol' error.
				throw new ArgumentException("Can't set a TypedAttribute as a TypedAttribute");
			} else {
				_mimetype = MimeClr(_contents != null ? _contents.GetType() : null);
			}
		}
	}

	/// <summary>
	/// Get/set the mime type of this attribute. Set only works for byte[] objects.
	/// </summary>
	public string mimetype {
		get {
			// We'll only ever not have a mime type if we're null or a byte[]
			// that never got its mime type set. In that case, set a default until
			// a value is set on us one way or another.
			if (_mimetype == null) {
				if (_contents is byte[])
					_mimetype = MimeBinary;
				if (_contents == null)
					_mimetype = MimeClr(null);
			}
			return _mimetype;
		}
		set {
			if (_contents == null || _contents is byte[])
				_mimetype = value;
			else
				throw new InvalidOperationException("Can't set the mime type for non-byte[] data");
		}
	}
	object _contents;
	string _mimetype;

	/// <summary>
	/// Attribute permissions. Only R, W, and C are possible.
	/// </summary>
	/// <remarks>
	/// Defaults are R+C.
	/// </remarks>
	public Perm perms {
		get { return _perms; }
		set {
			if (value & ~(Perm.R | Perm.W | Perm.C))
				throw new InvalidOperationException("Only R, W, and C permissions are valid for attributes");
			_perms = value;
		}
	}
	int _perms = Perm.R | Perm.C;

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
	public string str {
		get { return getContents<string>(); }
	}

	/// <summary>
	/// Returns contents as if we were a Mob.Ref value.
	/// </summary>
	public Mob.Ref mobref {
		get { return getContents<Mob.Ref>(); }
	}

	/// <summary>
	/// Returns true if we are a string.
	/// </summary>
	public bool isString { get { return _mimetype.EqualsI(MimeString); } }

	/// <summary>
	/// Returns true if we are a Mob.Ref.
	/// </summary>
	public bool isMobRef { get { return _mimetype.EqualsI(MimeMob); } }

	/// <summary>
	/// Returns true if we are an image.
	/// </summary>
	public bool isImage { get { return _mimetype.StartsWithI(MimeImagePrefix); } }

	/// <summary>
	/// Returns true if we are null.
	/// </summary>
	public bool isNull { get { return _mimetype.EqualsI(MimeNull); } }

	/// <summary>
	/// Retrieve the contents of this attribute as an array of bytes.
	/// </summary>
	/// <remarks>
	/// Items which are already byte[] will be returned directly, and strings are
	/// UTF8 byte encoded, but everything else is run through the .NET serializer.
	/// </remarks>
	public byte[] contentsAsBytes {
		get {
			var toserialize = _contents;

			if (_mimetype == MimeString)
				return Encoding.UTF8.GetBytes((string)toserialize);
			else if (_mimetype.EqualsI(MimeMob))
				toserialize = ((Mob.Ref)_contents).id;
			else if( _mimetype.EqualsI( MimeMobs ) )
				toserialize = ((object[])_contents).Select( m => ((Mob.Ref)m).id ).ToArray();
			else if (!_mimetype.StartsWithI(MimeClrPrefix))
				return toserialize as byte[];

			// If we make it here, it means we have a random .NET object to serialize.
			if( toserialize == null )
				return null;

			var ser = new BinaryFormatter();
			var stream = new MemoryStream();
			ser.Serialize(stream, toserialize);
			return stream.ToArray();
		}
	}

	/// <summary>
	/// Gets a safe-to-display-as-text string for our value.
	/// </summary>
	public string display {
		get {
			if (this.isString)
				return "\"{0}\"".FormatI(this.str);
			else if (this.isMobRef)
				return "<Mob: #{0}>".FormatI(this.mobref.id);
			else if( this.mimetype.EqualsI( MimeMobs ) )
				return "<Mob[]: #" + String.Join( ",", ((object[])this.contents).Select( m => ((Mob.Ref)m).id ).ToArray() ) + ">";
			else if (this.mimetype.StartsWithI("clr/"))
				return "<{0}: {1}>".FormatI(this.contents.GetType().Name, this.contents.ToStringI());
			else
				return "<binary data>";
		}
	}

	/// <summary>
	/// Converts any (supported) attribute value into a boxed, well-typed
	/// value that can be persisted.
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
	/// Create a typed attribute from a binary data stream and a mime type.
	/// </summary>
	/// <remarks>
	/// This is pretty much expected to be used when loading persisted data.
	/// </remarks>
	static public TypedAttribute FromPersisted(byte[] data, string mimetype) {
		if (mimetype.EqualsI(MimeString))
			return FromValue(Encoding.UTF8.GetString(data));
		if (!mimetype.EqualsI(MimeMob) && !mimetype.EqualsI( MimeMobs ) && !mimetype.StartsWithI(MimeClrPrefix))
			return new TypedAttribute() { contents = data, mimetype = mimetype };

		// Everything else is either mob/objectref or clr/...
		var ser = new BinaryFormatter();
		var stream = new MemoryStream(data);
		var obj = ser.Deserialize(stream);

		if (mimetype == MimeMob)
			return new TypedAttribute() { contents = new Mob.Ref((int)obj) };
		else if( mimetype == MimeMobs )
			return new TypedAttribute() { contents = ((int[])obj).Select( m => new Mob.Ref( m ) ).ToArray() };
		else
			return new TypedAttribute() { contents = obj };
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
}

}
