namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kayateia.Climoo.Database;

/// <summary>
/// Handles stuffing models data from an XML export into an existing database.
/// This can be done once for a proper database backing, or at load time for
/// memory database usage.
/// </summary>
public class XmlModelPersistence {
	static public void Import(string sourceFn, IDatabase target) {
		XmlClimooWeb web = XmlPersistence.Load<XmlClimooWeb>(sourceFn);

		foreach (var s in web.screens) {
			target.insert(Screen.Table, new Dictionary<string, object>() {
				{ "name", s.name },
				{ "text", s.text }
			});
		}

		foreach (var u in web.users) {
			target.insert(User.Table, new Dictionary<string, object>() {
				{ "login", u.login },
				{ "openid", u.openId },
				{ "password", u.password },
				{ "objectid", u.objectId == 0 ? null : (int?)u.objectId },
				{ "name", u.name }
			});
		}
	}
}

}
