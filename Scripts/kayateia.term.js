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

Term = {
	///////////////////////////////////////////////////
	// Global terminal settings
	settings: {
		prompt:			"climoo&gt; ",
		cursorSpeed:	500,
		commandHandler:	null
	},

	///////////////////////////////////////////////////
	// AJAX spinner
	spinner: {
		_cmdCount: 0,

		// Start an AJAX command and return its ID and a ready-to-go spinner.
		start: function() {
			var thisCmd = ++this._cmdCount;
			var spinnerCode = $('#input-spinner-template').clone();
			spinnerCode
				.attr('id', 'spinner-' + thisCmd)
				.fadeIn(100);
			return { 'id':thisCmd, 'dom':spinnerCode };
		},

		// Finish an AJAX command and take the spinner out of commission.
		finish: function(cmdId) {
			$('#spinner-' + cmdId).fadeOut(100, function() {
				$(this).remove();
			});
		}
	},

	///////////////////////////////////////////////////
	// Scroll handling
	scroll: {
		// Scroll by however many pages (-/+)
		scroll: function(pages) {
			var display = $('.terminal');
			display.animate({
				scrollTop: display.scrollTop() + pages * (display.height() * .75)
			}, 100, 'linear');
		},

		// Scroll to the bottom.
		toBottom: function() {
			var display = $('.terminal');
			display.animate({
				scrollTop: display.attr('scrollHeight')
			}, 100, 'linear');
		}
	},

	///////////////////////////////////////////////////
	// Input handler
	input: {
		_curLine: "",
		_cursorPos: 0,

		_update: function() {
			// Find the left half of the line.
			var left = this._curLine.substring(0, this._cursorPos);
			var onCursor = "&nbsp;";
			var right = "";
			if (this._cursorPos < this._curLine.length) {
				onCursor = this._curLine.substring(this._cursorPos, this._cursorPos + 1);
				right = this._curLine.substring(this._cursorPos + 1, this._curLine.length);
			}

			$('#input-left').html(left);
			$('#input-cursor').html(onCursor);
			$('#input-right').html(right);
		},

		set: function(newval) {
			this._curLine = newval;
			this._update();
		},

		get: function() {
			return this._curLine;
		},

		_checkOvershoot: function() {
			if (this._cursorPos > this._curLine.length)
				this._cursorPos = this._curLine.length;
		},

		setCursorPos: function(pos) {
			this._cursorPos = pos;
			this._update();
		},

		getCursorPos: function() {
			return this._cursorPos;
		},

		left: function() {
			this._checkOvershoot();
			if (--this._cursorPos < 0)
				this._cursorPos = 0;
			this._update();
		},

		right: function() {
			this._checkOvershoot();
			if (++this._cursorPos > this._curLine.length)
				this._cursorPos = this._curLine.length;
			this._update();
		},

		insert: function(ch) {
			this._checkOvershoot();
			if (this._cursorPos == this._curLine.length)
				this._curLine += ch;
			else if (this._cursorPos == 0)
				this._curLine = ch + this._curLine;
			else
				this._curLine =
					this._curLine.substring(0, this._cursorPos)
					+ ch
					+ this._curLine.substring(this._cursorPos, this._curLine.length);
			++this._cursorPos;
			this._update();
		},

		backspace: function() {
			this._checkOvershoot();
			if (this._cursorPos > 0) {
				this._curLine = this._curLine.substring(0, this._cursorPos - 1)
					+ this._curLine.substring(this._cursorPos, this._curLine.length);
				--this._cursorPos;
			}
			this._update();
		}
	},

	///////////////////////////////////////////////////
	// Command history handling
	history: {
		_commands: [],
		_idx: 0,
		_savedLine: [],

		add: function(line) {
			this._commands.push(line);
			this._idx = this._commands.length;
		},

		up: function() {
			if (this._idx == 0)
				return;
			if (this._idx == this._commands.length)
				this._savedLine = Term.input.get();
			else {
				if (this._commands[this._idx].length == Term.input.getCursorPos())
					Term.input.setCursorPos(this._commands[this._idx - 1].length);
			}

			Term.input.set(this._commands[--this._idx]);
		},

		down: function() {
			if (this._idx == this._commands.length)
				return;
			if (++this._idx == this._commands.length) {
				Term.input.set(this._savedLine);
				this._savedLine = "";
				return;
			} else {
				if (this._commands[this._idx - 1].length == Term.input.getCursorPos())
					Term.input.setCursorPos(this._commands[this._idx].length);
			}

			Term.input.set(this._commands[this._idx]);
		}
	},

	///////////////////////////////////////////////////
	// Output processing
	write: function(text, needSpinner) {
		$('#term-text').append(text);

		var spinnerId;
		if (needSpinner) {
			var spinnerInfo = Term.spinner.start();
			spinnerId = spinnerInfo['id'];
			$('#term-text').append(spinnerInfo['dom']);
		}
		$('#term-text').append('<br/>');
		Term.scroll.toBottom();

		return spinnerId;
	},

	writeCommand: function(text, needSpinner) {
		return Term.write('<span class="old-command"><span class="prompt">'
			+ Term.settings.prompt
			+ '</span>'
			+ text
			+ '</span>', needSpinner);
	},

	///////////////////////////////////////////////////
	// Command processing handling
	exec: function(commandText) {
		if (!commandText)
			return;

		Term.history.add(commandText);
		if (Term.settings.commandHandler)
			Term.settings.commandHandler(commandText);
		else
			Term.write("No command processor.", false);
	},

	///////////////////////////////////////////////////
	// Global init -- call from document.ready
	init: function() {
		// Set the prompt.
		$('#input-prompt').html(Term.settings.prompt);

		// Cursor flashing
		$('.cursor-flash').everyTime(500, "cursor-flash", function () {
			$(this).toggleClass('on');
		});

		$(document).bind('keydown', 'pageup', function(evt) {
			Term.scroll.scroll(-1);
			return false;
		})
		.bind('keydown', 'pagedown', function(evt) {
			Term.scroll.scroll(1);
			return false;
		})
		.bind('keypress', 'return', function(evt) {
			var execLine = Term.input.get();
			Term.input.set("");
			Term.exec(execLine);
			return false;
		})
		.bind('keydown', 'left', function(evt) {
			Term.input.left();
			return false;
		})
		.bind('keydown', 'right', function(evt) {
			Term.input.right();
			return false;
		})
		.bind('keydown', 'up', function(evt) {
			Term.history.up();
			return false;
		})
		.bind('keydown', 'down', function(evt) {
			Term.history.down();
			return false;
		})
		.bind('keypress', 'backspace', function(evt) {
			Term.input.backspace();
			return false;
		})
		.keypress(function (evt) {
			if (evt.which >= 32 && evt.which <= 126) {
				var ch = String.fromCharCode(evt.which);
				if (ch) {
					evt.preventDefault();
					Term.input.insert(String.fromCharCode(evt.which));
				}
			}
		});
	}
};

// AJAX functionality for the terminal; this adds the ability to
// send commands to the web server for execution, as well as a long-running
// "push" query for text coming back asynchronously.
TermAjax = {
	settings: {
		execUrl:		"/Game/ExecCommand",
		pushUrl:		"/Game/PushCheck"
	},

	// Executes the command on the server via AJAX, with a nice spinner.
	exec: function(commandText) {
		var spinnerId = Term.writeCommand(commandText, true);
		$.getJSON(TermAjax.settings.execUrl + "?cmd="
			+ escape(commandText)
			+ "&datehack=" + new Date().getTime(),
			function (data) {
				Term.spinner.finish(spinnerId);
				if (data.resultText)
					Term.write(data.resultText);
			}
		);
	},

	// Handle unrequested input from server -- this uses a long-poll
	// AJAX request (30 seconds). If something fires, it will return
	// immediately with results, and we will query again immediately;
	// otherwise the timeout will happen and we'll start again.
	pushBegin: function() {
		function errorFunction(xhr, status, err) {
			if (status == "timeout") {
				TermAjax.pushBegin();
			} else {
				// Wait a bit on error, in case something is flooded.
				alert("error" + status + " " + err);
				$(document).oneTime(3000, "push-reset", function() {
					TermAjax.pushBegin();
				});
			}
		}
		$.ajax({
			url: TermAjax.settings.pushUrl + "?datehack=" + new Date().getTime(),
			dataType: 'json',
			data: {},
			success:
				function (data) {
					if (data.resultText)
						Term.write(data.resultText);
					TermAjax.pushBegin();
				},
			error: errorFunction,
			timeout: 30000
		});
	},

	init: function() {
		Term.settings.commandHandler = TermAjax.exec;
		TermAjax.pushBegin();
	}
};

// Local terminal command handlers; this is pretty much just a proof
// of concept at this point.
TermLocal = {
	init: function() {
		var oldHandler = Term.settings.commandHandler;
		Term.settings.commandHandler = function(cmd) {
			if (cmd.substr(0, 6) == "local ") {
				Term.writeCommand(cmd);
				Term.write("Hey, you typed " + cmd.substr(6, cmd.length));
			} else
				oldHandler(cmd);
		};
	}
};

// Activate the terminal.
$(document).ready(function() {
	Term.init();
	TermAjax.init();
	TermLocal.init();
});

