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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;

// XML persistence support. This is intended to import and export the database rather
// than being a standard load/save method, because not only will it be slow compared
// to SQL, it has no checkpoint support.
public partial class World {
	// These inner classes represent a simple XML persistence structure for exporting the database.
	[DataContract(Namespace = "", Name="XmlClimoo")]
	class XmlClimoo {
		public XmlClimoo() {
			mobs = new List<XmlMob>();
		}

		[DataMember]
		public List<XmlMob> mobs { get; set; }
	}

	[DataContract(Namespace = "", Name="XmlMob")]
	class XmlMob {
		public XmlMob() {
			attrs = new List<XmlAttr>();
			verbs = new List<XmlVerb>();
		}

		[DataMember]
		public int id { get; set; }
		[DataMember]
		public int parentId { get; set; }
		[DataMember]
		public string pathId { get; set; }
		[DataMember]
		public int locationId { get; set; }
		[DataMember]
		public int permMask { get; set; }
		[DataMember]
		public int ownerId { get; set; }

		[DataMember]
		public List<XmlAttr> attrs { get; set; }
		[DataMember]
		public List<XmlVerb> verbs { get; set; }
	}

	[DataContract(Namespace = "", Name="XmlAttr")]
	class XmlAttr {
		[DataMember]
		public string mimeType { get; set; }
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public string textContents { get; set; }
		[DataMember]
		public string dataContentName { get; set; }
		[DataMember]
		public int permMask { get; set; }
	}

	[DataContract(Namespace = "", Name="XmlVerb")]
	class XmlVerb {
		[DataMember]
		public string name { get; set; }
		[DataMember]
		public string code { get; set; }
		[DataMember]
		public int permMask { get; set; }
	}

	/// <summary>
	/// Loads the world database from an XML persistence store.
	/// </summary>
	static public World FromXml(string baseDir) {
		World w = new World();
		w.importFromXml(baseDir);
		return w;
	}

	/// <summary>
	/// Loads the world database from an XML persistence store.
	/// </summary>
	public void importFromXml(string baseDir) {
		lock (_mutex)
			importFromXmlInner(baseDir);
	}

	void importFromXmlInner(string baseDir) {
		string binDir = Path.Combine(baseDir, "bins");

		XmlClimoo root = XmlPersistence.Load<XmlClimoo>(Path.Combine(baseDir, "mobs.xml"));

		// Divine the next id from the existing ones.
		_nextId = -1;
		foreach (XmlMob m in root.mobs)
			if (m.id > _nextId)
				_nextId = m.id;
		++_nextId;

		// Load up the mobs.
		foreach (XmlMob m in root.mobs) {
			var mob = new Mob(this, m.id) {
				parentId = m.parentId,
				locationId = m.locationId,
				ownerId = m.ownerId,
				pathId = m.pathId,
				perms = m.permMask
			};

			// Load up its attributes.
			foreach (XmlAttr attr in m.attrs) {
				TypedAttribute ta;
				if (attr.textContents != null)
					ta = TypedAttribute.FromValue(attr.textContents);
				else {
					byte[] data = File.ReadAllBytes(Path.Combine(binDir, attr.dataContentName));
					ta = TypedAttribute.FromPersisted(data, attr.mimeType);
				}
				ta.perms = attr.permMask;
				mob.attrSet(attr.name, ta);
			}

			// Load up its verbs.
			foreach (var verb in m.verbs) {
				Verb v = new Verb() {
					name = verb.name,
					code = verb.code,
					perms = verb.permMask
				};
				mob.verbSet(verb.name, v);
			}

			_objects[mob.id] = mob;
		}
	}

	/// <summary>
	/// Exports this world to an XML persistence store.
	/// </summary>
	public void exportToXml(string baseDir) {
		lock (_mutex)
			exportToXmlInner(baseDir);
	}

	void exportToXmlInner(string baseDir) {
		// We have a directory structure, not just an XML file, because we may also need to
		// store binary blobs like images.
		if (Directory.Exists(baseDir))
			Directory.Delete(baseDir, true);
		Directory.CreateDirectory(baseDir);

		string binDir = Path.Combine(baseDir, "bins");
		Directory.CreateDirectory(binDir);

		var objs = _objects.Values;

		XmlClimoo root = new XmlClimoo();

		foreach (Mob m in objs) {
			XmlMob mob = new XmlMob() {
				id = m.id,
				parentId = m.parentId,
				pathId = m.pathId,
				locationId = m.locationId,
				permMask = m.perms.mask,
				ownerId = m.ownerId
			};
			root.mobs.Add(mob);

			foreach (StringI name in m.attrList) {
				string strval = null, binfn = null;
				var item = m.attrGet(name);
				if (item.isString)
					strval = item.str;
				else {
					binfn = String.Format("{0}-{1}.bin", m.id, name);
					File.WriteAllBytes(Path.Combine(binDir, binfn), item.contentsAsBytes);
				}

				XmlAttr attr = new XmlAttr() {
					mimeType = item.mimetype,
					name = name,
					textContents = strval,
					dataContentName = binfn,
					permMask = item.perms.mask,
				};
				mob.attrs.Add(attr);
			}

			foreach (var name in m.verbList) {
				var item = m.verbGet(name);
				XmlVerb verb = new XmlVerb() {
					name = item.name,
					code = item.code,
					permMask = item.perms.mask,
				};
				mob.verbs.Add(verb);
			}
		}

		XmlPersistence.Save<XmlClimoo>(Path.Combine(baseDir, "mobs.xml"), root);
	}
}

}
