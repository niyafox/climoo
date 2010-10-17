namespace Kayateia.Climoo.Session {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Derive from this class to avoid the goofy global session lock. Instead,
/// our global request handler will pull our piece of the session and stuff
/// it into the Request.Items dictionary for us to use.
/// </summary>
public class SessionFreeController : Controller {
	public SessionFreeController() {
	}

	protected Session.UserContext _user {
		get {
			if (_userCache == null)
				_userCache = SessionManager.GetContext(HttpContext);
			return _userCache;
		}
	}
	Session.UserContext _userCache;
}

}
