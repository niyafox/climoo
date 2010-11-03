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
	/// <summary>
	/// Get/set the contents of this attribute. The mime type will also be set
	/// unless the value is byte[] (in which case we expect the user to set it).
	/// </summary>
	public object contents {
		get { return _contents; }
		set {
			_contents = value;
			if (_contents is string || _contents is StringI)
				_mimetype = "text/plain";
			else if (_contents is byte[]) {
				// We let the user set this so we don't trample on an object initializer.
			} else if (_contents is Mob.Ref) {
				// We store this by ID later.
				_mimetype = "moo/objectref";
			} else if (_contents is TypedAttribute) {
				// Big ol' error.
				throw new ArgumentException("Can't set a TypedAttribute as a TypedAttribute");
			} else {
				if (_contents == null)
					_mimetype = "clr/null";
				else
					_mimetype = "clr/{0}".FormatI(_contents.GetType().FullName);
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
					_mimetype = "application/octet-stream";
				if (_contents == null)
					_mimetype = "clr/null";
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
	public bool isString { get { return _mimetype.EqualsI("text/plain"); } }

	/// <summary>
	/// Returns true if we are a Mob.Ref.
	/// </summary>
	public bool isMobRef { get { return _mimetype.EqualsI("mob/objectref"); } }

	/// <summary>
	/// Returns true if we are an image.
	/// </summary>
	public bool isImage { get { return _mimetype.StartsWithI("image/"); } }

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

			if (_mimetype == "text/plain")
				return Encoding.UTF8.GetBytes((string)toserialize);
			else if (_mimetype.EqualsI("mob/objectref"))
				toserialize = ((Mob.Ref)_contents).id;
			else if (!_mimetype.StartsWithI("clr/"))
				return toserialize as byte[];

			// If we make it here, it means we have a random .NET object to serialize.
			var ser = new BinaryFormatter();
			var stream = new MemoryStream();
			ser.Serialize(stream, toserialize);
			return stream.ToArray();
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
		if (!mimetype.EqualsI("text/plain"))
			return FromValue(Encoding.UTF8.GetString(data));
		if (!mimetype.EqualsI("mob/objectref") && !mimetype.StartsWithI("clr/"))
			return new TypedAttribute() { contents = data, mimetype = mimetype };

		var ser = new BinaryFormatter();
		var stream = new MemoryStream(data);
		var obj = ser.Deserialize(stream);

		if (mimetype == "mob/objectref")
			return new TypedAttribute() { contents = new Mob.Ref((int)obj) };
		else
			return new TypedAttribute() { contents = obj };
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
				rv.mimetype = "application/octet-stream";
		}

		return rv;
	}
}

}
