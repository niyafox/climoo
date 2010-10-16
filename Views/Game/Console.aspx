<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head>
	<title>Game Console</title>
	<script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.timers-1.0.0.js"></script>
	<script type="text/javascript">
		$(document).ready(function () {
			// Cursor size and flashing
			var charW = $('.size-test').width();
			var charH = $('.size-test').height();
			$('.size-test').hide();

			$('.cursor-size').css({
				'width': charW + 'px',
				'height': charH + 'px'
			});
			$('.cursor-flash').everyTime(500, "cursor-flash", function () {
				$(this).toggleClass('on');
			});

			// Terminal widths to something predictable
			var termW = Math.floor(($(window).width() - 30) / charW);
			var termH = Math.floor($('.display-area').height() / charH);
			$('#terminal').css({
				'width': (termW * charW + 4) + 'px',
				'height': (termH * charH + 4) + 'px'
			});

			$('#input').css({
				'width': (termW * charW + 4) + 'px',
				'height': (charH + 4) + 'px'
			});

			$('.cursor-size').css({
				'left': '2px',
				'top': '2px'
			});
			$('#input-cursor').css({
				'left': (2 + charW) + 'px'
			});

			var cmdCount = 0;
			function commandStart() {
				if (++cmdCount == 1)
					$('#input-spinner').fadeIn(100);
			}

			function commandFinish() {
				if (--cmdCount == 0)
					$('#input-spinner').fadeOut(100);
			}

			// Input handler
			var curLine = "";
			$(document).keypress(function (evt) {
				if (evt.which == 13) {
					var execLine = curLine; curLine = "";
					commandStart();
					$.getJSON("/Game/ExecCommand?cmd=" + escape(execLine), function (data) {
						commandFinish();
						writeOutput(data.resultText);
					});
				} else if (evt.which == 8) {
					curLine = curLine.substring(0, curLine.length - 1);
				} else
					curLine += String.fromCharCode(evt.which);
				$('#input-text').html('&gt;' + curLine);
				$('#input-cursor').css({
					'left': (charW * (1+curLine.length) + 2) + 'px'
				});

				evt.stopImmediatePropagation();
			});

			// Output handler
			var cursorText = $('#term-text').html();
			var curText = [];
			function writeOutput(text) {
				curText.push(text);
				var output = "";
				for (i = 0; i < curText.length; ++i)
					output += curText[i] + '<br/>';
				$('#term-text').html(output + cursorText);
				$('#output-cursor').css({
					'left': '2px',
					'top': (charH * curText.length + 2) + 'px'
				});
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
					url: "/Game/PushCheck",
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
	</script>
	<style type="text/css">
		body {
			background-color: #004;
			color: #888;
		}
		.debug {
			border-style: solid;
			border-width: 1px;
		}
		.display-area {
			position: relative;
			height: 90%;
		}
		.term {
			background-color: #000;
			color: #aaa;
			position: relative;
			font-family: Terminal, Consolas, Fixed;
			font-size: 1em;
			left: 10px;
			padding: 2px 2px 2px 2px;
		}
		.term-text {
			margin: 0px 0px 0px 0px;
			padding: 0px 0px 0px 0px;
			width: 100%;
			height: 100%;
			position: relative;
			z-index: 2;
		}
		
		.size-test {
			font-family: Terminal, Consolas, Fixed;
			font-size: 1em;
		}
		
		.cursor-output {
			/* position: absolute;
			left: 0px; right: 0px;
			z-index: 1; */
			border-style: dotted;
			border-width: 1px;
			border-color: #aaa;
		}
		.cursor {
			position: absolute;
			left: 2px;
			top: 2px;
			z-index: 5;
		}
		.cursor.on {
			background-color: #aaa;
			color: #000;
		}
		
		.header-box {
			margin: 0px 0px 0px 0px;
			padding: 0px 0px 0px 0px;
			position: relative;
		}
		h1 {
			font-size: 1.2em;
			font-weight: bold;
		}
		.spinner {
			display: none;
			position: absolute;
			right: 0px;
			top: 0px;
			background-image: url(/Content/loadinfo.net.gif);
			background-position: left center;
			background-repeat: no-repeat;
			width: 16px;
			height: 16px;
		}
	</style>
</head>
<body>
	<div class="header-box">
		<h1>Game Terminal</h1>
		<div id="input-spinner" class="spinner"></div>
	</div>
	<div class="display-area">
		<div id="terminal" class="term">
			<div id="term-text" class="term-text">
				<div id="output-cursor" class="cursor-output cursor-size"></div>
			</div>
		</div>
	</div>
	<div id="input" class="term">
		<div id="input-cursor" class="cursor cursor-size cursor-flash"></div>
		<div id="input-text" class="term-text">&gt;</div>
	</div>
	<span class="size-test">@ </span>
</body>
</html>
