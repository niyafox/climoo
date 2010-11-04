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
	</style>
	<link rel="stylesheet" href="/Content/modalpopup.css" />
	<script type="text/javascript" src="/Scripts/kayateia.modalpopup.js"></script>
	<script type="text/javascript">
		$(document).ready(function() {
			popup = new ModalPopup('.modalpopup', 600, 400);
			$('body > div').click(function() {
				popup.popup();
			});
			$('.modalpopup .cancelbtn').click(function() {
				popup.popdown();
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

		<div class="modalpopup">
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
