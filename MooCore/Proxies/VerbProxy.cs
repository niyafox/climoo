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



	readonly SourcedItem<Verb> _verb;
	readonly Player _player;
}

}
