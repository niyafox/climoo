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

	readonly SourcedItem<Verb> _verb;
	readonly Player _player;
}

}
