/*

Editor popups for Climoo
Copyright (C) 2010 Kayateia

Requires:
	jquery-x.x.x.js
	jquery.timers-1.0.0.js
    kayateia.term.js
	kayateia.modalpopup.js
*/

ObjectEditor = {
	ajaxUrlGet: "/Game/GetObject",
	ajaxUrlSet: "/Game/SetObject",
	_popup: null,

	init: function() {
		ObjectEditor._popup = new ModalPopup('#objeditor');
		TermLocal.setHandler("`edit ", true, function(cmd, spn) {
			objname = cmd.substr(6, cmd.length - 6);
			Term.write("Looking up object '" + objname + "'...");

			$.getJSON(ObjectEditor.ajaxUrlGet + "?objectId="
				+ escape(objname)
				+ "&datehack=" + new Date().getTime(),
				function (data) {
					spn.finish();
					if (data.valid) {
						$('#objeditor .left').text("Editing '" + escape(objname) + "'");
						$('#objeditor .editid').val(data.id);
						$('#objeditor .editname').val(data.name);
						$('#objeditor .editpath').val(data.pathid);
						$('#objeditor .editparent').val(data.parent);
						$('#objeditor .editdesc').text(data.desc);
						ObjectEditor._popup.popup();
						Term.active = false;
						$('#objeditor .editdesc').focus();
					} else
						Term.write("Object was not valid.");
				}
			);
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
						Term.write("Object was saved.");
						ObjectEditor._popup.popdown();
						Term.active = true;
					}
				},
				error: function(req, status, err) {
					Term.write("Error saving: " + err);
				}
			});
		});

		$('#objeditor .cancelbtn').click(function() {
			ObjectEditor._popup.popdown();
			Term.active = true;
		});
	}
};

$(document).ready(function() {
	ObjectEditor.init();
});

$(document).ready(function() {
	TermLocal.setHandler("local ", false, function(cmd) {
		Term.write("Hey, you typed " + cmd.substr(6, cmd.length));
	});
});
