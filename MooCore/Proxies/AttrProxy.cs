namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// MOO Proxy object for a MOO object attribute. This is available to MOO scripts.
/// </summary>
public class AttrProxy : DynamicObjectBase {
	public AttrProxy(SourcedItem<TypedAttribute> attr, Player player) {
		_attr = attr;
		_player = player;
	}

	public AttrProxy(TypedAttribute attr, Player player) {
		_attr = new SourcedItem<TypedAttribute>(null, "<anon>", attr);
		_player = player;
	}

	/// <summary>
	/// The MOO object which owns this verb.
	/// </summary>
	/// <value>A proxy object for the mob in question.</value>
	[Passthrough]
	public MobProxy source {
		get { return new MobProxy(_attr.source, _player); }
	}

	/// <summary>
	/// The name of this attribute.
	/// </summary>
	[Passthrough]
	public string name {
		get { return _attr.name; }
	}

	/// <summary>
	/// The MIME type of this attribute's value.
	/// </summary>
	[Passthrough]
	public string mime {
		get { return _attr.item.mimetype; }
	}

	/// <summary>
	/// A display string for the attribute's value.
	/// </summary>
	[Passthrough]
	public string display {
		get { return _attr.item.display; }
	}

	/// <summary>
	/// The raw bytes comprising the attribute's value.
	/// </summary>
	[Passthrough]
	public byte[] bytes {
		get { return _attr.item.contentsAsBytes; }
	}

	/// <summary>
	/// The attribute's permissions.
	/// </summary>
	[Passthrough]
	public int perms {
		get {
			return _attr.item.perms;
		}
		set {
			// TODO: Only allow some perm changes depending on user
			_attr.item.perms = value;
		}
	}

	readonly SourcedItem<TypedAttribute> _attr;
	readonly Player _player;
}

}
