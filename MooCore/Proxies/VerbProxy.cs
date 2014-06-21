namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// MOO Proxy object for a MOO script verb. This is available to MOO scripts.
/// </summary>
public class VerbProxy : DynamicObjectBase {
	public VerbProxy(SourcedItem<Verb> verb, Player player) {
		_verb = verb;
		_player = player;
	}

	/// <summary>
	/// The MOO object which owns this verb.
	/// </summary>
	/// <value>A proxy object for the mob in question.</value>
	[Passthrough]
	public MobProxy source {
		get { return new MobProxy(_verb.source, _player); }
	}

	/// <summary>
	/// The name of the verb.
	/// </summary>
	[Passthrough]
	public string name {
		get { return _verb.name; }
	}

	/// <summary>
	/// The code for the verb.
	/// </summary>
	[Passthrough]
	public string code {
		get { return _verb.item.code; }
	}

	/// <summary>
	/// The permissions bitmask for the verb.
	/// </summary>
	[Passthrough]
	public int perms {
		get {
			return _verb.item.perms;
		}
		set {
			// TODO: Only allow some perm changes depending on user
			_verb.item.perms = value;
		}
	}

	readonly SourcedItem<Verb> _verb;
	readonly Player _player;
}

}
