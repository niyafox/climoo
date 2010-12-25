namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// Provides really simple constants for use in scripts.
/// It's fine to just pull the static instance of this
/// and put that into every script; nothing is writable.
/// </summary>
public class PermProxy : DynamicObjectBase {
	private PermProxy() { }
	static public PermProxy Static {
		get {
			return s_perm;
		}
	}
	static PermProxy s_perm = new PermProxy();

	[Passthrough]
	public int r { get { return Perm.R; } }

	[Passthrough]
	public int w { get { return Perm.W; } }

	[Passthrough]
	public int f { get { return Perm.F; } }

	[Passthrough]
	public int x { get { return Perm.X; } }

	[Passthrough]
	public int p { get { return Perm.P; } }

	[Passthrough]
	public int c { get { return Perm.C; } }

	[Passthrough]
	public int coder { get { return Perm.Coder; } }

	[Passthrough]
	public int mayor { get { return Perm.Mayor; } }

	[Passthrough]
	public int player { get { return Perm.Player; } }
}

}
