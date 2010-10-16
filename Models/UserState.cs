namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Represents the user's current state. This is typically a per-login
/// sort of thing (stored in the session) and not the user's long term game state.
/// </summary>
public class UserState {
	static public string SessionID = "UserState";

	public UserState() {
	}

	public string pendingOutput = "";
}

}