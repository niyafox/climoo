namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// A fully MIME-typed attribute blob. All attributes should either be strings, or
/// be objects of this type. Anything that's not a string should have a MIME type
/// associated with it and be serializable.
/// </summary>
public class TypedAttribute {
	public object contents;
	public string mimetype;

	public T getContents<T>() {
		return (T)this.contents;
	}

	public byte[] contentsAsBytes {
		get {
			// Not entirely sure what the right magic is here for later.
			if (this.contents is string)
				return Encoding.Default.GetBytes(this.contents as string);
			else if (this.contents is byte[])
				return this.contents as byte[];
			else
				throw new InvalidCastException("Contents can't be represented as byte stream");
		}
	}

	static Dictionary<string, string> ExtensionMap = new Dictionary<string,string> {
		{ "jpg", "image/jpeg" },
		{ "gif", "image/gif" },
		{ "png", "image/png" }
	};

	static public TypedAttribute LoadFromFile(string path) {
		byte[] contents;
		using (FileStream fs = File.OpenRead(path)) {
			int size = (int)fs.Length;
			contents = new byte[size];
			fs.Read(contents, 0, size);
		}

		var rv = new TypedAttribute();
		rv.contents = contents;

		string ext = Path.GetExtension(path);
		if (ext.StartsWith(".")) ext = ext.Substring(1);
		if (ExtensionMap.ContainsKey(ext))
			rv.mimetype = ExtensionMap[ext];

		return rv;
	}
}

}
