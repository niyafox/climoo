<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
<!--
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
    along with this program.  If not, see http://www.gnu.org/licenses/.
-->
	<link rel="Stylesheet" href="/Content/game.css" />
	<link rel="Stylesheet" href="/Content/modalpopup.css" />
	<style type="text/css">
		body {
			background-color: #557;
		}
		/* For some reason these won't cascade */
		span.modalpopup td {
			color: #fff;
		}
	</style>
</head>
<body>
	<span class="modalpopup"><span class="body">
		<form enctype="multipart/form-data" method="post" action="/Game/SetBinaryAttribute">
			<table border="0" cellpadding="5">
				<tr>
					<td>Object Name:</td>
					<td><input type="text" name="objectId" /></td>
				</tr>
				<tr>
					<td>Attribute:</td>
					<td><input type="text" name="name" /></td>
				</tr>
				<tr>
					<td>Mime Type (optional):</td>
					<td><input type="text" name="mimetype" /></td>
				</tr>
				<tr>
					<td>File:</td>
					<td><input type="file" name="fileData" /></td>
				</tr>
				<tr>
					<td></td>
					<td><input type="submit" value="Save" /></td>
				</tr>
			<% if (!Model.initial) { %>
				<tr>
					<td>Result:</td>
					<td><%= Model.message %></td>
				</tr>
			<% } %>
			</table>
		</form>
	</span></span>
</body>
</html>
