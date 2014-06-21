namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Extension methods for the .NET String class.
/// </summary>
static public class StringCase {
	/// <summary>
	/// A culture and case insensitive version of .Contains<string>().
	/// </summary>
	static public bool ContainsI(this IEnumerable<string> self, string item) {
		return (from i in self where item.Equals(i, StringComparison.OrdinalIgnoreCase) select 0).Any();
	}

	/// <summary>
	/// A culture and case insensitive version of String.StartsWith().
	/// </summary>
	static public bool StartsWithI(this string self, string prefix) {
		return self.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// A culture and case insensitive version of String.Equals().
	/// </summary>
	static public bool EqualsI(this string self, string other) {
		if ((self == null || other == null)
			&& (self != null || other != null))
		{
			return false;
		}
		return self.Equals(other, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// A culture free version of String.Format().
	/// </summary>
	static public string FormatI(this string self, params object[] param) {
		return String.Format(CultureFree.Culture, self, param);
	}

	/// <summary>
	/// A culture free version of Object.ToString().
	/// </summary>
	static public string ToStringI(this object self) {
		return "{0}".FormatI(self);
	}

	/// <summary>
	/// Implement string.IsNullOrEmpty as an "instance method".
	/// </summary>
	static public bool IsNullOrEmpty(this string str) {
		return string.IsNullOrEmpty(str);
	}
}

}
