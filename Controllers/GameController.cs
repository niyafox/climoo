namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Trace = System.Diagnostics.Trace;
using Kayateia.Climoo.Models;

/// <summary>
/// Controls the main game screen / loop.
/// </summary>
public class GameController : Session.SessionFreeController {
	// The actual main page view.
	public ActionResult Index() {
		/* if (_user.player == null) {
			string result = Game.Login.LogUserIn(_user, "kayateia", "");
			if (result != null) {
				_user.outputPush("Could not log you in: " + result);
				return View("Console");
			}
		}

		MooCore.InputParser.ProcessInput("look", _user.player); */

		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output.
	public JsonResult PushCheck() {
		// Wait for new output, and fail if we don't get any by 25 seconds.
		string newText;
		if (!_user.outputWait(25000))
			newText = "";
		else {
			// Get what's there, if anything is left.
			newText = _user.outputPopAll();
		}

		var result = new Models.ConsoleCommand() {
			resultText = newText
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Called by the page when the user types a command. This may return
	// data immediately rather than waiting for the push.
	public JsonResult ExecCommand(string cmd) {
		string output;
		try {
			output = _user.inputPush(cmd);
		} catch (System.Exception ex) {
			output = "<span class=\"error\">Exception: {0}</span>".FormatI(ex.Message);
		}
		var result = new Models.ConsoleCommand() {
			resultText = output
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	[OutputCache(NoStore=true, Duration=0, VaryByParam="")]
	public ActionResult ServeAttribute(int objectId, string attributeName) {
		if (!_user.inGame)
			return null;

		MooCore.Mob mob = Game.WorldData.world.findObject(objectId);
		if (mob == null)
			return null;

		var attr = mob.findAttribute(attributeName);
		return this.File(attr.contentsAsBytes, attr.mimetype);
	}

	/* public ActionResult Editor() {
		return View("EditorTest");
	} */

	public JsonResult GetObject(string objectId) {
		if (!_user.inGame)
			return null;

		MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, _user.player.avatar);
		MooCore.Mob parent = obj.parent;
		string parentId = "";
		if (parent != null && parent.id > 0) {
			string fqpn = parent.fqpn;
			if (!string.IsNullOrEmpty(fqpn))
				parentId = fqpn;
			else
				parentId = "#{0}".FormatI(obj.parentId);
		}
		var result = new {
			valid = obj != MooCore.Mob.None && obj != MooCore.Mob.Ambiguous,
			id = obj.id,
			name = obj.name,
			parent = parentId,
			pathid = obj.pathId,
			desc = obj.desc
		};
		return Json(result, JsonRequestBehavior.AllowGet);
	}

	[HttpPost]
	public JsonResult SetObject(int? id, string name, string parent, string pathid, string desc) {
		if (!_user.inGame)
			return null;

		object result;
		try {
			MooCore.Mob obj;
			if (!id.HasValue)
				obj = Game.WorldData.world.createObject(new {}, location:_user.player.avatar.locationId);
			else
				obj = Game.WorldData.world.findObject(id.Value);
			if (obj == null || obj == MooCore.Mob.None)
				result = new { valid = false, message = "Invalid object ID" };
			else {
				int? parentId = null;
				if (parent.StartsWithI("#"))
					parentId = CultureFree.ParseInt(parent.Substring(1));
				else if (parent.StartsWithI(MooCore.Mob.PathSep+""))
					parentId = obj.world.findObject(parent).id;

				obj.name = name;
				if (parentId.HasValue)
					obj.parentId = parentId.Value;
				obj.pathId = pathid;
				obj.desc = desc;

				result = new { valid = true, message = "", id = obj.id };
			}
		} catch (Exception ex) {
			result = new { valid = false, message = ex.Message };
		}
		return Json(result, JsonRequestBehavior.DenyGet);
	}

	public JsonResult GetVerb(string objectId, string verb) {
		if (!_user.inGame)
			return null;

		object result;

		MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, _user.player.avatar);
		if (obj == MooCore.Mob.None) {
			result = new { valid = false, message = "Unknown object" };
		} else if (obj == MooCore.Mob.Ambiguous) {
			result = new { valid = false, message = "Ambiguous object" };
		} else {
			MooCore.Verb v = obj.verbGet(verb);
			if (v == null)
				result = new { valid = true, message = "Unknown verb", id = obj.id, code = "" };
			else {
				result = new { valid = true, message = "",
					id = obj.id,
					code = v.code
				};
			}
		}

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	[HttpPost]
	public JsonResult SetVerb(int objectId, string verb, string code) {
		if (!_user.inGame)
			return null;

		object result;

		MooCore.Mob obj = Game.WorldData.world.findObject(objectId);
		if (obj == null) {
			result = new { valid = false, message = "Unknown object" };
		} else {
			string message = "";
			bool valid = true;
			try {
				MooCore.Verb v = new MooCore.Verb() {
					name = verb,
					code = code
				};
				obj.verbSet(verb, v);
			} catch (System.Exception ex) {
				message = "<span class=\"error\">Exception: {0}</span>".FormatI(ex.Message);
				valid = false;
			}
			result = new { valid = valid, message = message };
		}

		return Json(result, JsonRequestBehavior.DenyGet);
	}

	public ActionResult UploadFrame() {
		if (!_user.inGame)
			return null;

		dynamic result = new System.Dynamic.ExpandoObject();
		result.initial = true;
		return View("UploadBinaryFrame", result);
	}

	[HttpPost]
	public ActionResult SetBinaryAttribute(string objectId, string name, string mimetype, HttpPostedFileBase fileData) {
		if (!_user.inGame)
			return null;

		dynamic result = new System.Dynamic.ExpandoObject();
		result.initial = false;

		MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, _user.player.avatar);
		if (obj == MooCore.Mob.None) {
			result.message = "Unknown object";
		} else if (obj == MooCore.Mob.Ambiguous) {
			result.message = "Ambiguous object";
		} else {
			if (fileData.ContentLength > 500*1024)
				result = new { valid = false, message = "File too large" };
			byte[] bytes = new byte[fileData.ContentLength];
			fileData.InputStream.Read(bytes, 0, fileData.ContentLength);

			if (string.IsNullOrEmpty(mimetype))
				mimetype = fileData.ContentType;

			var ta = new MooCore.TypedAttribute() {
				contents = bytes,
				mimetype = mimetype
			};
			obj.attrSet(name, ta);

			result.message = "Save was successful!";
		}

		return View("UploadBinaryFrame", result);
	}
}

}
