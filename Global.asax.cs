namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
// visit http://go.microsoft.com/?LinkId=9394801

public class MvcApplication : System.Web.HttpApplication {
	public static void RegisterRoutes(RouteCollection routes) {
		routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

		routes.MapRoute(
			"Default", // Route name
			"{controller}/{action}/{id}", // URL with parameters
			new { controller = "Game", action = "Index", id = UrlParameter.Optional } // Parameter defaults
		);
	}

	protected void Application_Start() {
		AreaRegistration.RegisterAllAreas();
		RegisterRoutes(RouteTable.Routes);

		// Automatically delete timed out sessions in our table.
		_reaperQuit = new System.Threading.ManualResetEvent(false);
		_reaper = new System.Threading.Thread(new System.Threading.ThreadStart(() => {
			while (!_reaperQuit.WaitOne(30000)) {
				Climoo.Session.SessionManager.GrimReaper();
			}
		}));
		_reaper.Start();

		Game.WorldData.Init();
	}

	protected void Application_End() {
		if (_reaper != null) {
			_reaperQuit.Set();
			_reaper.Join();
			_reaper = null;
			_reaperQuit.Dispose();
			_reaperQuit = null;
		}
	}

	protected void Application_BeginRequest() {
		// In order to avoid the dumb session lock queueing, we need to
		// disabled the session ID cookie to trick MVC into not blocking
		// for the request(s).
		if (/*Request.Url.AbsoluteUri.ToLower().Contains("/game/")
			&& */ Request.Cookies["ASP.NET_SessionId"] != null)
		{
			Request.Cookies.Remove("ASP.NET_SessionId");
		}
	}

	System.Threading.ManualResetEvent _reaperQuit;
	System.Threading.Thread _reaper;
}

}
