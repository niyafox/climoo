namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Thin wrapper for String that provides case and culture
/// insensitive comparison operators.
/// </summary>
public struct StringI {
	public StringI(string s) { _s = s; }

	public override int GetHashCode() {
		return _s.ToLowerInvariant().GetHashCode();
	}

	public override string ToString() {
		return _s;
	}

	public override bool Equals(object obj) {
		if (!(obj is StringI) && !(obj is string))
			return false;

		string b;
		if (obj is StringI)
			b = ((StringI)obj)._s;
		else
			b = obj as string;

		return _s.EqualsI(b);
	}

	static public bool operator==(StringI a, StringI b) {
		return a._s.EqualsI(b._s);
	}

	static public bool operator!=(StringI a, StringI b) {
		return !(a == b);
	}

	static public StringI operator+(StringI a, StringI b) {
		return new StringI(a._s + b._s);
	}

	static public implicit operator StringI(string si) {
		return new StringI(si);
	}

	static public implicit operator string(StringI si) {
		return si._s;
	}

	string _s;
}

}
