﻿/*
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

namespace Kayateia.Climoo.Scripting.SSharp {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptNET.Runtime;

/// <summary>
/// Information about a member of a dynamic object.
/// </summary>
internal class DynamicMemberInfo : System.Reflection.MemberInfo {
	public DynamicMemberInfo(IDynamicObject target, string name) {
		_target = target;
		_name = name;
	}

	public override Type DeclaringType {
		get {
			return _target.GetType();
		}
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
		return new object[0];
	}

	public override object[] GetCustomAttributes(bool inherit) {
		return new object[0];
	}

	public override bool IsDefined(Type attributeType, bool inherit) {
		return false;
	}

	public override System.Reflection.MemberTypes MemberType {
		get {
			return System.Reflection.MemberTypes.Property;
		}
	}

	public override string Name {
		get {
			return _name;
		}
	}

	public override Type ReflectedType {
		get {
			return _target.getMember(_name).GetType();
		}
	}

	IDynamicObject _target;
	string _name;
}

/// <summary>
/// Represents a successful binding to a member in an IDynamicObject.
/// </summary>
internal class DynamicMemberBinding : IMemberBind {
	public DynamicMemberBinding(IDynamicObject target, string name) {
		_target = target;
		_name = name;
	}

	public object Target {
		get { return _target; }
	}

	public Type TargetType {
		get { return _target.GetType(); }
	}

	public System.Reflection.MemberInfo Member {
		get { return new DynamicMemberInfo(_target, _name); }
	}

	public void SetValue(object value) {
		_target.setMember(_name, value);
	}

	public object GetValue() {
		return _target.getMember(_name);
	}

	public void AddHandler(object value) {
		throw new NotImplementedException();
	}

	public void RemoveHandler(object value) {
		throw new NotImplementedException();
	}

	public bool CanInvoke() {
		return false;
	}

	public object Invoke(IScriptContext context, object[] args) {
		throw new NotImplementedException();
	}

	IDynamicObject _target;
	string _name;
}

}
