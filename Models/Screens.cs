namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Screens {
	static public string Get(string name) {
		using (var db = new ClimooDataContext()) {
			db.Connection.Open();

			var row = from r in db.GetTable<Screen>()
						where r.name == name
						select r;

			if (!row.Any())
				return "";
			else
				return row.First().text;
		}
	}
}

}