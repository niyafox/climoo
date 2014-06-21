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

namespace Kayateia.Climoo.MooCore {
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// Utility class for enumerating properties on an object.
/// </summary>
internal class PropertyEnumerator {
    static public IEnumerable<PropertyValue> GetProperties(object o) {
        if (o != null) {
            var props = TypeDescriptor.GetProperties(o);
            foreach (PropertyDescriptor prop in props) {
                object val = prop.GetValue(o);
                if (val != null) {
                    yield return new PropertyValue { Name = prop.Name, Value = val };
                }
            }
        }
    }

	public sealed class PropertyValue {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}

}
