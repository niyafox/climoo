<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head>
	<title>climoo :: terminal</title>
	<script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.timers-1.0.0.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.hotkeys.js"></script>
	<script type="text/javascript" src="/Scripts/kayateia.term.js"></script>
	<link rel="Stylesheet" href="/Content/term.css" />
	<link rel="Stylesheet" href="/Content/game.css" />
	<style type="text/css">
		body {
			background-color: #004;
			color: #888;
		}
	</style>
</head>
<body>
	<div class="header-box">
		<h1>climoo :: terminal</h1>
	</div>
	<div class="terminal themed">
		<div id="term-text"></div>
		<div id="input">
			<!-- These have to stay on one line not to trigger the 'pre' whitespace -->
			<span id="input-prompt" class="prompt"></span><span id="input-left"></span><span id="input-cursor" class="cursor cursor-size cursor-flash">&nbsp;</span><span id="input-right"></span>
		</div>
	</div>
	<img id="input-spinner-template" class="input-spinner" src="/Content/spiral-spinner-000.gif" alt="[spinner]" />
</body>
</html>
