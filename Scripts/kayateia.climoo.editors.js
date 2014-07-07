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

/*

Editor popups for Climoo

Requires:
	jquery-x.x.x.js
	jquery.timers-1.0.0.js
    kayateia.term.js
	kayateia.modalpopup.js
*/

ObjectEditor = {
	ajaxUrlGet: "/Game/GetObject",
	ajaxUrlSet: "/Game/SetObject",

	init: function() {
		TermLocal.setHandler("!edit ", true, function(cmd, spn) {
			objname = cmd.substr(6, cmd.length - 6);
			Term.write("Looking up object '" + objname + "'...");

			$.ajax({
				url: ObjectEditor.ajaxUrlGet + "?objectId="
						+ escape(objname)
						+ "&datehack=" + new Date().getTime(),
				dataType: 'json',
				success: function(data) {
					spn.finish();
					if (data.valid) {
						data.title = "Editing '" + escape(objname) + "'";
						ObjectEditor.loadEditor(data);
						ObjectEditor.popEditor(true);
					} else
						Term.write("Object was not valid.");
				},
				error: TermAjax.standardErrorHandler(spn),
				timeout: 30000
			});
		});

		TermLocal.setHandler("!create", false, function(cmd) {
			data = {
				title: "Create new object",
				id: "",
				name: "",
				pathid: "",
				parent: "",
				desc: ""
			};
			ObjectEditor.loadEditor(data);
			ObjectEditor.popEditor(true);
		});

		$('#objeditor .savebtn').click(function() {
			data = $('#objeditor form').serialize();
			$.ajax({
				type: "POST",
				url: ObjectEditor.ajaxUrlSet,
				dataType: "json",
				data: data,
				success: function(data) {
					if (!data.valid)
						Term.write("Error saving: " + data.message);
					else {
						Term.write("Object (#" + data.id + ") was saved.");
						ObjectEditor.popEditor(false);
					}
				},
				error: function(req, status, err) {
					Term.write("Error saving: " + err);
				}
			});
		});

		$('#objeditor .cancelbtn').click(function() {
			ObjectEditor.popEditor(false);
		});

		$('#objeditor .close').click(function() {
			ObjectEditor.popEditor(false);
		});

		$('#objeditor').on('shown.bs.modal', function(e) {
			Term.active = false;
			$('#texteditor .edittext').focus();
		});

		$('#objeditor').on('hidden.bs.modal', function (e) {
			Term.active = true;
		});
	},

	loadEditor: function(data) {
		$('#objeditor .modal-title').text(data.title);
		$('#objeditor .editid').val(data.id);
		$('#objeditor .editname').val(data.name);
		$('#objeditor .editpath').val(data.pathid);
		$('#objeditor .editparent').val(data.parent);
		$('#objeditor .editdesc').text(data.desc);
	},

	popEditor: function(up) {
		if (up) {
			$('#objeditor').modal();
			$('#objeditor .editdesc').focus();
		} else {
			$('#objeditor').modal('hide');
		}
	}
};

$(document).ready(function() {
	ObjectEditor.init();
});

// Imported from http://www.scottklarr.com/topic/425/how-to-insert-text-into-a-textarea-where-the-cursor-is/
function insertAtCaret(txtarea,text) { var scrollPos = txtarea.scrollTop; var strPos = 0; var br = ((txtarea.selectionStart || txtarea.selectionStart == '0') ? "ff" : (document.selection ? "ie" : false ) ); if (br == "ie") { txtarea.focus(); var range = document.selection.createRange(); range.moveStart ('character', -txtarea.value.length); strPos = range.text.length; } else if (br == "ff") strPos = txtarea.selectionStart; var front = (txtarea.value).substring(0,strPos); var back = (txtarea.value).substring(strPos,txtarea.value.length); txtarea.value=front+text+back; strPos = strPos + text.length; if (br == "ie") { txtarea.focus(); var range = document.selection.createRange(); range.moveStart ('character', -txtarea.value.length); range.moveStart ('character', strPos); range.moveEnd ('character', 0); range.select(); } else if (br == "ff") { txtarea.selectionStart = strPos; txtarea.selectionEnd = strPos; txtarea.focus(); } txtarea.scrollTop = scrollPos; } 

TextEditor = {
	_callback: null,

	init: function() {
		function saveCancelCommon(saving) {
			var id = $('#texteditor .editid').val();
			var text = $('#texteditor .edittext').val();
			if (TextEditor._callback(id, text, saving))
				$('#texteditor').modal('hide');
		}

		$('#texteditor .savebtn').click(function() {
			saveCancelCommon(true);
		});

		$('#texteditor .cancelbtn').click(function() {
			saveCancelCommon(false);
		});

		$('#texteditor .close').click(function() {
			saveCancelCommon(false);
		});

		$('#texteditor').on('shown.bs.modal', function(e) {
			Term.active = false;
			$('#texteditor .edittext').focus();
		});

		$('#texteditor').on('hidden.bs.modal', function (e) {
			Term.active = true;
		});

		$('#texteditor textarea').bind('keydown', function(evt) {
			if (evt.keyCode == 9) {
				var txted = $('#texteditor textarea')[0];
				insertAtCaret(txted, "\t");
				return false;
			}
		});
	},

	popdown: function() {
		$('#texteditor').modal('hide');
	},

	// Callback should be in this form:
	// function[bool] callback(id, text, success[bool]);
	// If it returns true, the editor will pop down.
	edit: function(title, id, text, callback) {
		$('#texteditor .modal-title').text(title);
		$('#texteditor .editid').val(id);
		$('#texteditor .edittext').val(text);
		TextEditor._callback = callback;

		$('#texteditor').modal();
	}
};

$(document).ready(function() {
	TextEditor.init();
});

VerbEditor = {
	ajaxUrlGet: "/Game/GetVerb",
	ajaxUrlSet: "/Game/SetVerb",
	init: function() {
		TermLocal.setHandler("!verb ", true, function(cmd, spn) {
			var rest = cmd.substr(6, cmd.length - 6);
			var objectIdx = rest.indexOf(" ");
			var verbName = rest.substr(0, objectIdx);
			var objName = rest.substr(objectIdx+1, rest.length - (objectIdx+1));
			Term.write("Looking up verb '" + verbName + "' on object '" + objName + "'...");

			$.ajax({
				url: VerbEditor.ajaxUrlGet
						+ "?objectId=" + escape(objName)
						+ "&verb=" + escape(verbName)
						+ "&datehack=" + new Date().getTime(),
				dataType: 'json',
				success:function (data) {
					spn.finish();
					if (data.valid) {
						title = "Editing verb '" + escape(verbName) + "' on object '" + escape(objName) + "'";
						TextEditor.edit(title, data.id + "." + verbName, data.code,
							function(id, text, success) {
								if (success) {
									chunks = id.split(".");

									$.ajax({
										type: "POST",
										url: VerbEditor.ajaxUrlSet,
										dataType: "json",
										data: {
											objectId: chunks[0],
											verb: chunks[1],
											code: text
										},
										success: function(data) {
											if (data.valid) {
												Term.write("Verb was written.");
												TextEditor.popdown();
											} else
												Term.write("Verb was not written: " + data.message);
										},
										error: function(req, status, err) {
											Term.write("Error saving: " + status + "/" + err);
										},
										timeout: 30000
									});
								} else
									return true;
							}
						);
					} else
						Term.write("Request failed: " + data.message);
				},
				error: TermAjax.standardErrorHandler(spn),
				timeout: 30000
			});
		});
	}
};

$(document).ready(function() {
	VerbEditor.init();
});

UploadBinary = {
	ajaxUrl: "/Game/UploadFrame",

	init: function() {
		$('#uploader').on('hidden.bs.modal', function (e) {
			Term.active = true;
		});

		$('#uploader .cancelbtn').click(function() {
			$('#uploader').modal('hide');
			$('#uploader .body').html('');
		});

		$('#uploader .savebtn').click(function() {
			$('#UploadBinaryFrame').contents().find('#submitbtn').click();
		});

		TermLocal.setHandler("!upload", false, function(cmd) {
			Term.active = false;
			$('#uploader .body').html('<iframe id="UploadBinaryFrame" width="570" height="300" src="' + UploadBinary.ajaxUrl + '" frameborder="0" />');
			$('#uploader').modal();
		});
	}
};

$(document).ready(function() {
	UploadBinary.init();
});

LoginBox = {
	init: function() {
		$('#loginbox input[type!="button"]').bind('keydown', 'return', function(evt) {
			$('#loginbox .loginbtn').click();
		});

		$('#loginbox .loginbtn').click(function() {
			login = $('#loginbox .login').val();
			$('#loginbox .login').val("");
			pass = $('#loginbox .password').val();
			$('#loginbox .password').val("");

			$('#loginbox').modal('hide');

			TermAjax.exec("login " + login + " " + pass, "Logging in...");
		});

		$('#loginbox').on('shown.bs.modal', function(e) {
			$('#loginbox .login').focus();
		});

		$('#loginbox').on('hidden.bs.modal', function (e) {
			Term.active = true;
		});

		TermLocal.setHandler("login", false, function(cmd) {
			Term.active = false;
			$('#loginbox').modal();
		});
	}
};

$(document).ready(function() {
	LoginBox.init();
});

// This is nice for testing, but it's a XSS bug waiting to happen.
/*$(document).ready(function() {
	TermLocal.setHandler("local ", false, function(cmd) {
		Term.write("Hey, you typed " + cmd.substr(6, cmd.length));
	});
}); */
