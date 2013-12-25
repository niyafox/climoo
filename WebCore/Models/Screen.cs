namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a screen to be used in the pre-login area.
/// </summary>
public class Screen {
	public const string Table = "screen";

	public int id;
	public string name;
	public string text;

	static public Screen FromDatabase(IDictionary<string, object> values) {
		return new Screen() {
			id = (int)values["id"],
			name = (string)values["name"],
			text = (string)values["text"]
		};
	}
}

}
