namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Kayateia.Climoo.Database;

/// <summary>
/// Container for all screens.
/// </summary>
public class Screens {
	static public string Get(IDatabase db, string name) {
		var results = db.select(Screen.Table, new Dictionary<string, object>() {
			{ "name", name }
		});
		if (!results.Any())
			return "";
		else
			return (string)results.Values.First()["text"];
	}
}

}