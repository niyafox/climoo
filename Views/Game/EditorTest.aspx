<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>EditorTest</title>
	<script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
	<style type="text/css">
		body {
			margin: 0px 0px 0px 0px;
			padding: 0px 0px 0px 0px;
		}
		body > div {
			margin: 0px 0px 0px 0px;
			padding: 2px 2px 2px 2px;
			background-color: #99c;
			color: #555;
			font-weight: bold;
			position: relative;
		}

		div.editor {
			background-color: #779;
			border: 1px solid #446;
			display: block;
			left: -30px;
			padding: 10px;
			position: absolute;
			width: 0px;
			height: 0px;
			opacity: 0;

			-moz-border-radius:10px;
			-webkit-border-radius:10px;
			border-radius:10px;
			-webkit-box-shadow:rgba(68,68,68,.5) 0px 3px 8px;
		}

		.editor .title {
			background-color: #ddd;
			color: #444;
			font-weight: bold;
			font-family: Segoe UI, Sans-Serif;
			font-size: 15px;
			height: 20px;
			padding: 2px;
			margin: 0px;
			position: relative;
			vertical-align: middle;

			/* Give a cheap drop shadow appearance */
			border: 1px solid #ddd;
			border-bottom-color: #444;
			border-right-color: #444;
		}
		
		.editor .title .left {
			text-align: left;
			height: 100%;
			vertical-align: middle;
		}

		.editor .title .right {
			position: absolute;
			top: 0px;
			right: 5px;
			height: 100%;
			vertical-align: middle;
		}
		
		.editor .body {
			width: 10px;
			height: 10px;
			height: 270px;
			position: relative;
		}
		
		.editor .body textarea {
			width: 100%;
			height: 100%;
		}

		/* div.editor:after {
			border-color: #fff transparent transparent;
			border-style: solid;
			border-width: 10px 10px;
			bottom: -20px;
			content:"\00a0";
			display: block;
			height: 0;
			left: 75px;
			position: absolute;
			width: 0;
		} */
		
	</style>
	<script type="text/javascript">
		var edup = false;
		function popup() {
			if (edup) return;
			centerX = $(window).width() / 2;
			centerY = $(window).height() / 2;
			popW = 600;
			popH = 300;

			// Can't seem to get this to lay out right. This is simplest.
			$('.editor .body').css({
				width: (popW - 6) + 'px',
				height: (popH - 30) + 'px'
			});

			$('.editor').css({
				left: centerX + 'px',
				top: centerY + 'px'
			}).animate({
				width: popW + 'px',
				height: popH + 'px',
				left: (centerX - popW/2) + 'px',
				top: (centerY - popH/2) + 'px',
				opacity: 1.0
			}, 200);
			edup = true;
		}

		function popdown() {
			if (!edup) return;
			ed = $('.editor');
			centerX = ed.position().left + ed.width() / 2;
			centerY = ed.position().top + ed.height() / 2;
			$('.editor').animate({
				width: '0px',
				height: '0px',
				left: centerX + 'px',
				top: centerY + 'px',
				opacity: 0.0
			}, 200, 'swing', function() {
				$('.editor').hide();
				edup = false;
			});
		}

		$(document).ready(function() {
			$('body > div').click(function() {
				popup();
			});
			$('.editor .cancelbtn').click(function(evt) {
				popdown();
				evt.stopPropagation();
			});
		});
	</script>
</head>
<body>
    <div>
		<%
			for (int i=0; i<5000; ++i)
				Response.Write("quux ");
		%>

		<div class="editor">
			<div class="title">
				<div class="left">Editing: #5.look[code]</div>
				<div class="right"><input class="savebtn" type="button" value="Save"></input><input class="cancelbtn" type="button" value="Cancel"></input></div>
			</div>
			<div class="body">
				<textarea>
yay yay some code yay
here's some more stuff
woohoo!
				</textarea>
			</div>
		</div>
    </div>
</body>
</html>
