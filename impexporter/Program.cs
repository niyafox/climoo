namespace impexporter {
using System;
using Kayateia.Climoo.MooCore;

class Program {
	static void Import(string baseDir) {
		World w = World.FromXml(baseDir);
	}

	static void Export(string baseDir) {
		World w = World.FromSql();
		w.exportToXml(baseDir);
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
