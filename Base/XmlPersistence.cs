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

namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;

/// <summary>
/// Quick XML persistence load/save methods.
/// </summary>
public class XmlPersistence {
	/// <summary>
	/// Loads from a persisted XML file into a data model.
	/// </summary>
	static public T Load<T>(string filename) {
		using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			using (XmlReader reader = XmlReader.Create(stream)) {
				DataContractSerializer dcs = new DataContractSerializer(typeof(T));
				return (T)dcs.ReadObject(reader);
			}
	}

	/// <summary>
	/// Saves a data model into a persisted XML file.
	/// </summary>
	static public void Save<T>(string filename, T model) {
		using (FileStream stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
			using (XmlWriter writer = XmlWriter.Create(stream)) {
				DataContractSerializer dcs = new DataContractSerializer(typeof(T));
				dcs.WriteObject(writer, model);
			}
	}
}

}
