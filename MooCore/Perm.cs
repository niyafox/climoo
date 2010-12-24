namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The possible permissions; not all permissions are possible for
/// all objects to which they could be applied.
/// </summary>
public class Perm {
	public const int R = 1 << 0;			// Read [all]
	public const int W = 1 << 1;			// Write [all]
	public const int F = 1 << 2;			// Fertile [obj]
	public const int X = 1 << 3;			// Execute [verb]
	public const int P = 1 << 4;			// use from Prompt [verb]
	public const int C = 1 << 5;			// Changeable [property]

	public const int Coder = 1 << 6;		// Can create/change objects [player mobs only]
	public const int Mayor = 1 << 7;		// System admin [player mobs only]
	public const int Player = 1 << 8;		// Is a player mob [player mobs only]

	public Perm() {
		_mask = 0;
	}

	public Perm(int mask) {
		_mask = mask;
	}

	static public Dictionary<int, string> PermNames = new Dictionary<int,string> {
		{ R, "R" },
		{ W, "W" },
		{ F, "F" },
		{ X, "X" },
		{ P, "P" },
		{ C, "C" },
		{ Coder, "Coder" },
		{ Mayor, "Mayor" },
		{ Player, "Player" }
	};

	/// <summary>
	/// Returns true if the specified permissions contain any of the masked bits.
	/// </summary>
	public bool have(Perm mask) {
		return (this.mask & mask.mask) != 0;
	}

	public int mask {
		get { return _mask; }
		set { _mask = value; }
	}

	static public implicit operator Perm(int m) { return new Perm(m); }
	static public implicit operator int(Perm p) { return p._mask; }
	static public Perm operator ~(Perm p) { return new Perm(~p.mask); }
	static public Perm operator |(Perm a, Perm b) { return new Perm(a.mask | b.mask); }
	static public Perm operator |(Perm a, int b) { return new Perm(a.mask | b); }
	static public bool operator &(Perm a, Perm b) { return a.have(b.mask); }
	static public bool operator &(Perm a, int b) { return a.have(b); }

	int _mask;
}

}
