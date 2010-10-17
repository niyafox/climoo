namespace Kayateia.Climoo.Session {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Non-ASP session manager, to deal with session lock issues.
/// </summary>
public class SessionManager {
	static public string SessionCookieID = "Climoo_UserContext_ID";

	// FIXME: Session timeouts

	/// <summary>
	/// Retrieve a user context for the current user, either by
	/// creating it or by pulling an existing one.
	/// </summary>
	static public UserContext GetContext(HttpContextBase httpContext) {
		// Find or create the session ID cookie.
		var cookie = httpContext.Request.Cookies[SessionCookieID];
		if (cookie == null) {
			cookie = new HttpCookie(SessionCookieID, Guid.NewGuid().ToString("N")) {
				Expires = DateTime.UtcNow + new TimeSpan(7, 0, 0, 0)
			};
			httpContext.Response.SetCookie(cookie);
		}

		// Pull or create our session info.
		Guid key = new Guid(cookie.Value);
		if (s_sessions.ContainsKey(key))
			return s_sessions[key];
		else {
			s_sessions[key] = new UserContext();
			return s_sessions[key];
		}
	}

	static Dictionary<Guid, UserContext> s_sessions = new Dictionary<Guid,UserContext>();
}

}
