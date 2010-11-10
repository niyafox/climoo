namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

public class AttrProxy : DynamicObjectBase {
	public AttrProxy(SourcedItem<TypedAttribute> attr, Player player) {
		_attr = attr;
		_player = player;
	}

	[Passthrough]
	public MobProxy source {
		get { return new MobProxy(_attr.source, _player); }
	}

	[Passthrough]
	public string name {
		get { return _attr.name; }
	}

	[Passthrough]
	public string display {
		get { return _attr.item.display; }
	}



	readonly SourcedItem<TypedAttribute> _attr;
	readonly Player _player;
}

}
