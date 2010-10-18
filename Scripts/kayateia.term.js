/*

Javascript / JQuery based terminal interface for a web page
Copyright (C) 2010 Kayateia

This piece was definitely inspired by the xkcd CLI (uni.xkcd.com)
but it's not derivative of it.

*/

/*
Requires:
	jquery-x.x.x.js
	jquery.timers-1.0.0.js
	jquery.hotkeys.js
*/

/* Term = {
	// Global terminal settings
	settings: {
		prompt: "climoo&gt; ",
		cursorSpeed: 500
	},

	init: function() {
		// Set the prompt.
		$('#input-prompt').html(Term.settings.prompt);
	}
}; */

$(document).ready(function () {
	// Set the prompt.
	var prompt = "climoo&gt; ";
	$('#input-prompt').html(prompt);

	// AJAX spinner
	var cmdCount = 0;
	function commandStart() {
		var thisCmd = ++cmdCount;
		var spinnerCode = $('#input-spinner-template').clone();
		spinnerCode
			.attr('id', 'spinner-' + thisCmd)
			.fadeIn(100);
		return { 'id':thisCmd, 'dom':spinnerCode };
	}

	function commandFinish(cmdId) {
		$('#spinner-' + cmdId).fadeOut(100, function() {
			$(this).remove();
		});
	}

	// Cursor flashing
	$('.cursor-flash').everyTime(500, "cursor-flash", function () {
		$(this).toggleClass('on');
	});

	// Scroll handling
	function scroll(pages) {
		var display = $('.display-area');
		display.animate({
			scrollTop: display.scrollTop() + pages * (display.height() * .75)
		}, 100, 'linear');
	}
	function scrollToBottom() {
		var display = $('.display-area');
		display.animate({
			scrollTop: display.attr('scrollHeight')
		}, 100, 'linear');
	}
	$(document).bind('keydown', 'pageup', function(evt) {
		scroll(-1);
		return false;
	});
	$(document).bind('keydown', 'pagedown', function(evt) {
		scroll(1);
		return false;
	});

	// Input handler
	var curLine = "";
	var cursorPos = 0;
	function inputLineUpdate() {
		// Find the left half of the line.
		var left = curLine.substring(0, cursorPos);
		var onCursor = "&nbsp;";
		var right = "";
		if (cursorPos < curLine.length) {
			onCursor = curLine.substring(cursorPos, cursorPos + 1);
			right = curLine.substring(cursorPos + 1, curLine.length);
		}

		$('#input-left').html(left);
		$('#input-cursor').html(onCursor);
		$('#input-right').html(right);
	}
	function inputLineSet(newval) {
		curLine = newval;
		inputLineUpdate();
	}
	function inputLineCheckOvershoot() {
		if (cursorPos > curLine.length)
			cursorPos = curLine.length;
	}
	function inputLineLeft() {
		inputLineCheckOvershoot();
		if (--cursorPos < 0)
			cursorPos = 0;
		inputLineUpdate();
	}
	function inputLineRight() {
		inputLineCheckOvershoot();
		if (++cursorPos > curLine.length)
			cursorPos = curLine.length;
		inputLineUpdate();
	}
	function inputLineInsert(ch) {
		inputLineCheckOvershoot();
		if (cursorPos == curLine.length)
			curLine += ch;
		else if (cursorPos == 0)
			curLine = ch + curLine;
		else
			curLine = curLine.substring(0, cursorPos) + ch + curLine.substring(cursorPos, curLine.length);
		++cursorPos;
		inputLineUpdate();
	}
	function inputLineBackspace() {
		inputLineCheckOvershoot();
		if (cursorPos > 0) {
			curLine = curLine.substring(0, cursorPos - 1) + curLine.substring(cursorPos, curLine.length);
			--cursorPos;
		}
		inputLineUpdate();
	}

	var commandHistory = [];
	var commandHistoryIdx = 0;
	var commandHistorySavedLine = "";
	function historyAdd(line) {
		commandHistory.push(line);
		commandHistoryIdx = commandHistory.length;
	}
	function historyUp() {
		if (commandHistoryIdx == 0)
			return;
		if (commandHistoryIdx == commandHistory.length)
			commandHistorySavedLine = curLine;
		else {
			if (commandHistory[commandHistoryIdx].length == cursorPos)
				cursorPos = commandHistory[commandHistoryIdx - 1].length;
		}

		inputLineSet(commandHistory[--commandHistoryIdx]);
	}
	function historyDown() {
		if (commandHistoryIdx == commandHistory.length)
			return;
		if (++commandHistoryIdx == commandHistory.length) {
			inputLineSet(commandHistorySavedLine);
			commandHistorySavedLine = "";
			return;
		} else {
			if (commandHistory[commandHistoryIdx - 1].length == cursorPos)
				cursorPos = commandHistory[commandHistoryIdx].length;
		}

		inputLineSet(commandHistory[commandHistoryIdx]);
	}

	$(document).bind('keypress', 'return', function(evt) {
		var execLine = curLine;
		inputLineSet("");
		historyAdd(execLine);
		var spinnerId = writeOutput('<span class="old-command"><span class="prompt">' + prompt + '</span>' + execLine, execLine);
		if (execLine) {
			$.getJSON("/Game/ExecCommand?cmd="
				+ escape(execLine)
				+ "&datehack=" + new Date().getTime(),
				function (data) {
					commandFinish(spinnerId);
					if (data.resultText)
						writeOutput(data.resultText);
				}
			);
		}

		return false;
	});
	$(document).bind('keydown', 'left', function(evt) {
		inputLineLeft();
	});
	$(document).bind('keydown', 'right', function(evt) {
		inputLineRight();
	});
	$(document).bind('keydown', 'up', function(evt) {
		historyUp();
	});
	$(document).bind('keydown', 'down', function(evt) {
		historyDown();
	});
	$(document).bind('keypress', 'backspace', function(evt) {
		inputLineBackspace();
		return false;
	});
	$(document).keypress(function (evt) {
		if (evt.which >= 32 && evt.which <= 126) {
			var ch = String.fromCharCode(evt.which);
			if (ch) {
				evt.preventDefault();
				inputLineInsert(String.fromCharCode(evt.which));
			}
		}
	});

	// Output handler
	function writeOutput(text, needSpinner) {
		$('#term-text').append(text);
		var spinnerId;
		if (needSpinner) {
			var spinnerInfo = commandStart();
			spinnerId = spinnerInfo['id'];
			$('#term-text').append(spinnerInfo['dom']);
		}
		$('#term-text').append('<br/>');
		scrollToBottom();

		return spinnerId;
	}

	// Handle unrequested input from server -- this uses a long-poll
	// AJAX request (30 seconds). If something fires, it will return
	// immediately with results, and we will query again immediately;
	// otherwise the timeout will happen and we'll start again.
	function pushBegin() {
		function errorFunction(xhr, status, err) {
			if (status == "timeout") {
				pushBegin();
			} else {
				// Wait a bit on error, in case something is flooded.
				alert("error" + status + " " + err);
				$(document).oneTime(5000, "push-reset", function() {
					pushBegin();
				});
			}
		}
		$.ajax({
			url: "/Game/PushCheck" + "?datehack=" + new Date().getTime(),
			dataType: 'json',
			data: {},
			success:
				function (data) {
					if (data.resultText)
						writeOutput(data.resultText);
					pushBegin();
				},
			error: errorFunction,
			timeout: 30000
		});
	}
	pushBegin();
});
