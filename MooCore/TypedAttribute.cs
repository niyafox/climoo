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

	public T getContents<T>() {
		return (T)this.contents;
	}

	public string str {
		get { return getContents<string>(); }
	}

	public Mob.Ref mobref {
		get { return getContents<Mob.Ref>(); }
	}

	public bool isString { get { return _mimetype.EqualsI("text/plain"); } }
	public bool isMobRef { get { return _mimetype.EqualsI("mob/objectref"); } }
	public bool isImage { get { return _mimetype.StartsWithI("image/"); } }

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

	static public object AttributeFromPersisted(string data) {
		return data;
	}

	static public object AttributeFromPersisted(byte[] data, string mimetype) {
		var typed = FromPersisted(data, mimetype);
		if (typed.mimetype == "text/plain")
			return typed.contents;
		else
			return typed;
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
