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

		lock (s_lock) {
			if (s_sessions.ContainsKey(key))
				return s_sessions[key];
			else {
				s_sessions[key] = new UserContext();
				return s_sessions[key];
			}
		}
	}

	/// <summary>
	/// Goes through and looks for sessions past their due date, and kills them.
	/// The user will be logged out.
	/// </summary>
	static public void GrimReaper() {
		var reaped = new List<UserContext>();
		lock (s_lock) {
			DateTimeOffset now = DateTimeOffset.UtcNow;
			foreach (var pair in (from s in s_sessions
									where (now - s.Value.lastUse) > TimeoutTime
									select s).ToArray())
			{
				reaped.Add(pair.Value);
				s_sessions.Remove(pair.Key);
			}
		}

		foreach (var cxt in reaped) {
			// In case someone is watching right now.
			cxt.outputPush("<br/>Auto logged out due to timeout.<br/><br/>");

			Game.Login.LogUserOut(cxt);
			cxt.Dispose();
		}
	}

	static TimeSpan TimeoutTime = new TimeSpan(hours:1, minutes:0, seconds:0);
	static object s_lock = new object();
	static Dictionary<Guid, UserContext> s_sessions = new Dictionary<Guid,UserContext>();
}

}
