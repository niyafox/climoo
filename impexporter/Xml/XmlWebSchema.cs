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

// These classes represent an XML persistence form of data for the web site itself.

[DataContract(Namespace="")]
public class XmlClimooWeb {
	[DataMember]
	public List<XmlScreen> screens = new List<XmlScreen>();

	[DataMember]
	public List<XmlUser> users = new List<XmlUser>();
}

[DataContract(Namespace="")]
public class XmlScreen {
	[DataMember]
	public string name;

	[DataMember]
	public string text;
}

[DataContract(Namespace="")]
public class XmlUser {
	[DataMember]
	public string login;
	
	[DataMember]
	public bool openId;
	
	[DataMember]
	public string password;
	
	[DataMember]
	public int objectId;

	[DataMember]
	public string name;
}
}
