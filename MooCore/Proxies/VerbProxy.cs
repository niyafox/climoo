namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class VerbProxy : DynamicObjectBase {
	public VerbProxy(SourcedItem<Verb> verb, Player player) {
		_verb = verb;
		_player = player;
	}

	[Passthrough]
	public MobProxy source {
		get { return new MobProxy(_verb.source, _player); }
	}

	[Passthrough]
	public string name {
		get { return _verb.name; }
	}

	[Passthrough]
	public string code {
		get { return _verb.item.code; }
	}

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
