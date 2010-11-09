namespace Kayateia.Climoo.MooCore.Sql {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Doesn't actually work right now.
static public class SqlSetup {
	[STAThread]
	static void Main(string[] args) {
		Console.WriteLine("Creating SQL database...");
		using (var context = new Sql.MooCoreSqlDataContext()) {
			context.Connection.Open();
			context.CreateDatabase();
		}
		Console.WriteLine("Creating default world...");
		MooCore.World w = MooCore.World.CreateDefault();
		w.saveToSql();
		Console.WriteLine("Database created.");
	}
}

}
