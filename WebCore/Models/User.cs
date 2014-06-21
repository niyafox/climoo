namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a user for login.
/// </summary>
public class User {
	public const string Table = "user";

	public int id;
	public string login;
	public bool openid;
	public string password;
	public int? objectid;
	public string name;

	/// <summary>
	/// Loads a user from a database table row.
	/// </summary>
	static public User FromDatabase(IDictionary<string, object> values) {
		return new User() {
			id = (int)values["id"],
			login = (string)values["login"],
			openid = (bool)values["openid"],
			password = (string)values["password"],
			objectid = values["objectid"] == null ? null : (int?)values["objectid"],
			name = (string)values["name"]
		};
	}
}

}
