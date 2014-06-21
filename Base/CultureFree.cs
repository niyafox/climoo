namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Culture-insensitive parsing and such.
/// </summary>
static public class CultureFree {
	/// <summary>
	/// Quick access to the invariant culture for formatting and comparison.
	/// </summary>
	static public System.Globalization.CultureInfo Culture {
		get { return System.Globalization.CultureInfo.InvariantCulture; }
	}

	/// <summary>
	/// Culture-insensitive version of int.Parse().
	/// </summary>
	static public int ParseInt(string str) {
		return int.Parse(str, Culture);
	}
}

}
