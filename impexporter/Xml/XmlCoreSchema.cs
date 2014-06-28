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

namespace Kayateia.Climoo.ImpExporter.Xml
{
using System.Collections.Generic;
using System.Runtime.Serialization;

// These classes represent a simple XML persistence structure for importing and exporting the database.

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
}
