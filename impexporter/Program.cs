namespace impexporter {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Kayateia.Climoo;
using Kayateia.Climoo.Models;
using Kayateia.Climoo.MooCore;
using Legacy = Kayateia.Climoo.Models.LegacySql;

// This app loads up the world and web databases from MSSQL and exports them to XML files that
// can be loaded into the in-memory structures on Mono.
class Program {
	static void Import(string baseDir) {
		World w = World.FromXml(baseDir);
		Kayateia.Climoo.Database.IDatabase db = new Kayateia.Climoo.Database.MemoryDatabase();
		XmlModelPersistence.Import(@"d:\game\export\web.xml", db);
	}

	static void Export(string baseDir) {
		Console.WriteLine("Exporting world database...");
		World w = World.FromSql();
		w.exportToXml(baseDir);

		// This holds everything not in MooCore.
		XmlClimooWeb web = new XmlClimooWeb();

		Console.WriteLine("Exporting web core database...");
		using (var db = new Legacy.ClimooDataContext()) {
			db.Connection.Open();

			web.screens.AddRange(from r in db.GetTable<Legacy.Screen>()
				select new XmlScreen() {
					name = r.name,
					text = r.text
				});
			web.users.AddRange(from r in db.GetTable<Legacy.User>()
				select new XmlUser() {
					login = r.login,
					name = r.name,
					objectId = r.objectid.HasValue ? r.objectid.Value : 0,
					openId = r.openid,
					password = r.password
				});
		}

		XmlPersistence.Save<XmlClimooWeb>(Path.Combine(baseDir, "web.xml"), web);
	}

    static void Main(string[] args) {
		if (args.Length != 2) {
			Console.WriteLine("Please specify:");
			Console.WriteLine(" - 'import' or 'export'");
			Console.WriteLine(" - The name of a directory where the input will come from, or where output dump will go.");
			return;
		}

		if (args[0] == "import")
			Import(args[1]);
		else if (args[0] == "export")
			Export(args[1]);
		else {
			Console.WriteLine("Invalid operation '{0}'", args[0]);
			return;
		}
    }
}

}
