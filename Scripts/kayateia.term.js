﻿/*
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

Javascript / JQuery based terminal interface for a web page

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
		commandHandler:	null,
		sidebarHandler: null,
		soundHandler:	null
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
				.prop('id', 'spinner-' + thisCmd)
				.fadeIn(100);
			return { 'id':thisCmd, 'dom':spinnerCode };
		},

		// Finish an AJAX command and take the spinner out of commission.
		finish: function(cmdId, replaceWith) {
			$('#spinner-' + cmdId).fadeOut(100, function() {
				if (replaceWith)
					$(this).before(replaceWith);
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
				scrollTop: display.prop('scrollHeight')
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

		leftWord: function() {
			var prevSpace;

			// Find the nearest space to the left of the cursor,
			// excluding those immediately adjacent to the cursor.
			// Treat consecutive spaces as a single space.
			do {
				prevSpace = this._curLine.lastIndexOf(' ', --this._cursorPos - 1);
			} while (this._curLine.charAt(prevSpace + 1) == ' ');

			// Position the cursor immediately to the right of the
			// identified space, or the start of the line if there
			// are no further spaces.
			if (prevSpace == -1)
				this._cursorPos = 0;
			else
				this._cursorPos = prevSpace + 1;
			this._update();
		},

		leftToStart: function() {
			this._cursorPos = 0;
			this._update();
		},

		right: function() {
			this._checkOvershoot();
			if (++this._cursorPos > this._curLine.length)
				this._cursorPos = this._curLine.length;
			this._update();
		},

		rightWord: function() {
			var nextSpace;

			// Find the nearest space to the right of the cursor.
			// Treat consecutive spaces as a single space.
			do {
				nextSpace = this._curLine.indexOf(' ', ++this._cursorPos);
			} while (nextSpace != -1 && this._curLine.charAt(nextSpace - 1) == ' ')

			// Position the cursor at the identified space, or at
			// the end of the line if there are no further spaces.
			if (nextSpace == -1)
				this._cursorPos = this._curLine.length;
			else
				this._cursorPos = nextSpace;
			this._update();
		},

		rightToEnd: function() {
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

		del: function() {
			if (this._cursorPos < this._curLine.length)
				this._curLine = this._curLine.substring(0, this._cursorPos) + this._curLine.substring(this._cursorPos + 1);
			this._update();
		},

		delWord: function() {
			var delTo = this._cursorPos;
			this.leftWord();
			var delFrom = this._cursorPos;

			this._curLine = this._curLine.substring(0, delFrom) + this._curLine.substring(delTo);
			this._update();
		},

		delToStart: function() {
			this._curLine = this._curLine.substring(this._cursorPos);
			this._cursorPos = 0;
			this._update();
		},

		delToEnd: function() {
			var newLine = this._curLine.substring(0, this._cursorPos);
			this._curLine = newLine;
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
		var $outputBlock = $('<div class="output-block">' + text + '</div>');
		$('#term-text').append($outputBlock);

		var spinnerId;
		if (needSpinner) {
			var spinnerInfo = Term.spinner.start();
			spinnerId = spinnerInfo['id'];
			$outputBlock.append(spinnerInfo['dom']);
		}
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
	active: true,
	init: function() {
		// Set the prompt.
		$('#input-prompt').html(Term.settings.prompt);

		// Cursor flashing
		$('.cursor-flash').everyTime(500, "cursor-flash", function () {
			if (Term.active)
				$(this).toggleClass('on');
			else
				$(this).removeClass('on');
		});

		keybindings = [
			['keydown', 'pageup', function(evt) {
				Term.scroll.scroll(-1);
			}],

			['keydown', 'pagedown', function(evt) {
				Term.scroll.scroll(1);
			}],

			['keypress', 'return', function(evt) {
				var execLine = Term.input.get();
				Term.input.set("");
				Term.exec(execLine);
			}],

			['keydown', 'left', function(evt) {
				Term.input.left();
			}],

			['keydown', 'ctrl+left', function(evt) {
				Term.input.leftWord();
			}],

                        ['keydown', 'home', function(evt) {
                                Term.input.leftToStart();
                        }],

			['keydown', 'right', function(evt) {
				Term.input.right();
			}],

			['keydown', 'ctrl+right', function(evt) {
				Term.input.rightWord();
			}],

                        ['keydown', 'end', function(evt) {
                                Term.input.rightToEnd();
                        }],

			['keydown', 'up', function(evt) {
				Term.history.up();
			}],

			['keydown', 'down', function(evt) {
				Term.history.down();
			}],

			['keydown', 'ctrl+u', function(evt) {
				Term.input.delToStart();
			}],

			['keydown', 'ctrl+k', function(evt) {
				Term.input.delToEnd();
			}],

			['keydown', 'ctrl+w', function(evt) {
				Term.input.delWord();
			}],

			['keydown', 'del', function(evt) {
				Term.input.del();
			}],

			['keydown', 'backspace', function(evt) {
				Term.input.backspace();
			}]
		];

		for (var i=0; i<keybindings.length; ++i) {
			kb = keybindings[i];
			evthandler = { handler:kb[2] };
			$(document).bind(kb[0], kb[1], $.proxy(function(evt) {
				if (Term.active) {
					this.handler(evt);
					return false;
				} else
					return true;
			}, evthandler));
		}

		$(document).keypress(function(evt) {
			if (Term.active && !evt.ctrlKey) {
				if (evt.which >= 32 && evt.which <= 126) {
					var ch = String.fromCharCode(evt.which);
					if (ch) {
						evt.preventDefault();
						Term.input.insert(String.fromCharCode(evt.which));
					}
				}
			}
		});

		$('.terminal').prop('tabindex', 0);
		$('.terminal').blur(function(evt) {
			Term.active = false;
		});

		$('.terminal').focus(function(evt) {
			Term.active = true;
		});

		$('.terminal').focus();
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
	// If squelchText is non-null/empty, it will be printed instead of the command.
	exec: function(commandText, squelchText) {
		if (!squelchText)
			squelchText = commandText;
		var spinnerId = Term.writeCommand(squelchText, true);
		$.ajax({
			url: TermAjax.settings.execUrl + "?cmd="
					+ escape(commandText)
					+ "&datehack=" + new Date().getTime(),
			dataType: 'json',
			success: function(data) {
				Term.spinner.finish(spinnerId);
				TermAjax.handleResponse(data);
			},
			error: TermAjax.standardErrorHandler(spinnerId),
			timeout: 30000
		});
	},

	handleResponse: function(data) {
		if (data.text)
			Term.write(data.text);
		if (data.prompt)
			Term.settings.prompt = data.prompt;
		if (data.sidebar && Term.settings.sidebarHandler)
			Term.settings.sidebarHandler(data.sidebar);
		if (data.sound && Term.settings.soundHandler)
			Term.settings.soundHandler(data.sound);
	},

	// Generates an error handler for terminal-based AJAX requests.
	standardErrorHandler: function(spinnerId) {
		return $.proxy(function(xhr, status, err) {
			var msg;
			if (status == "timeout") {
				msg = "timed out"
			} else {
				msg = "server error";
			}

			// Deal with "spinner handles" handed out by TermLocal.
			var spinnerId = this;
			if ('id' in spinnerId)
				spinnerId = spinnerId.id;

			Term.spinner.finish(spinnerId, ' <span class="error">(' + msg + ')</span>');
		}, spinnerId);
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
					TermAjax.handleResponse(data);
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

// Local terminal command handlers.
//
// Functions should be in this form:
//   void func(command[, spinner])
// The spinner object will have a single method, finish().
TermLocal = {
	_handlers: {},

	init: function() {
		var oldHandler = Term.settings.commandHandler;
		Term.settings.commandHandler = function(cmd) {
			for (var key in TermLocal._handlers) {
				if (cmd.substr(0, key.length) == key) {
					hnd = TermLocal._handlers[key];
					spnid = Term.writeCommand(cmd, hnd.spinner);
					var spn;
					if (spnid) {
						spn = {
							id: spnid,
							finish: function() {
								Term.spinner.finish(spnid);
							}
						};
					}
					hnd.f(cmd, spn);
					return;
				}
			}
			oldHandler(cmd);
		};
	},

	setHandler: function(prefix, needsSpinner, helpText, func) {
		TermLocal._handlers[prefix] = { f:func, spinner:needsSpinner, help:helpText };
	},

	getHandlers: function() {
		return TermLocal._handlers;
	}
};

HelpHandler = {
	init: function() {
		TermLocal.setHandler("!help", false, "Shows this help.", function() {
			var output = "<table>";
			var handlers = TermLocal.getHandlers();
			for (var key in handlers)
				output += "<tr><td><span style=\"margin-right:10px; color:#5cc; font-weight:bold\">" + key + "</span></td><td>" + handlers[key].help + "</td></tr>";
			output += "</table>";
			Term.write(output, false);
		});
	}
};

// Sidebar handler. This just takes care of shuttling contents from AJAX calls
// back to the sidebar.
SidebarHandler = {
	init: function() {
		Term.settings.sidebarHandler = SidebarHandler.handle;
	},

	handle: function(data) {
		$('#sidebar').load(data);
	}
};

// Sound handler. Takes care of downloading and activating sounds by HTML5 API.
SoundHandler = {
	init: function() {
		Term.settings.soundHandler = SoundHandler.handle;

		try {
			// Fix up for prefixing
			window.AudioContext = window.AudioContext || window.webkitAudioContext;
			SoundHandler.context = new AudioContext();
		}
		catch(e) {
			// Web Audio API is not supported in this browser.
			SoundHandler.context = null;
		}
	},

	handle: function(url) {
		if (!SoundHandler.context)
			return;

		if (!SoundHandler.sounds[url]) {
			var request = new XMLHttpRequest();
				request.open('GET', url, true);
				request.responseType = 'arraybuffer';

				// Decode asynchronously
				request.onload = function() {
				SoundHandler.context.decodeAudioData(request.response, function(buffer) {
					SoundHandler.sounds[url] = buffer;
					SoundHandler.playSound(url);
				}, function() {
					alert("Error loading audio");
				});
			}
			request.send();
		} else {
			SoundHandler.playSound(url);
		}
	},

	playSound: function(url) {
		if (!SoundHandler.context)
			return;

		var source = SoundHandler.context.createBufferSource();		// creates a sound source
		source.buffer = SoundHandler.sounds[url];					// tell the source which sound to play
		source.connect(SoundHandler.context.destination);			// connect the source to the context's destination (the speakers)
		source.start(0);											// play the source now
	},

	context: null,
	sounds: {}
};

// Activate the terminal.
$(document).ready(function() {
	Term.init();
	TermAjax.init();
	TermLocal.init();
	SidebarHandler.init();
	SoundHandler.init();
	HelpHandler.init();
});

