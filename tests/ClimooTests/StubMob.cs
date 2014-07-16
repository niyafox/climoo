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

namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.Linq;
using Kayateia.Climoo.MooCore;

/// <summary>
/// Mock of the IMob object, for testing use.
/// </summary>
public class StubMob : IMob
{
	public StubMob( int id, int parentId )
	{
		this.id = id;
		this.parentId = parentId;
	}
	
	public int id { get; set; }

	public int parentId { get; set; }

	public int locationId { get; set; }

	public int ownerId { get; set; }

	public void verbSet( StringI name, Verb v ) { _verbs[name] = v; }
	public Verb verbGet( StringI name ) { return _verbs.ContainsKey( name ) ? _verbs[name] : null; }
	public void verbDel( StringI name ) { _verbs.Remove( name ); }
	public IEnumerable<StringI> verbList { get { return _verbs.Keys.ToArray(); } }
	Dictionary<StringI, Verb> _verbs = new Dictionary<StringI, Verb>();

	public void attrSet( StringI name, TypedAttribute v ) { _attrs[name] = v; }
	public TypedAttribute attrGet( StringI name ) { return _attrs.ContainsKey( name ) ? _attrs[name] : null; }
	public void attrDel( StringI name ) { _attrs.Remove( name ); }
	public IEnumerable<StringI> attrList { get { return _attrs.Keys.ToArray(); } }
	Dictionary<StringI, TypedAttribute> _attrs = new Dictionary<StringI, TypedAttribute>();

	public Player player { get; set; }

	public IWorld world { get; set; }
}

}
