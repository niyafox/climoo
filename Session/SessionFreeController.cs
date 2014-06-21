/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
