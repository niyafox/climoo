namespace Kayateia.Climoo.Models {
using System.Runtime.Serialization;
using System.Collections.Generic;

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