<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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

<style type="text/css">
	/* Stay invisible even if CSS isn't loaded */
	#uploader {
		display: none;
	}

	#uploader .body {
		width: 500px;
		height: 300px;
	}
</style>

<div id="uploader" class="modalpopup">
	<div class="title">
		<div class="left">Upload Binary Attribute</div>
		<div class="right">
			<input class="cancelbtn" type="button" value="Close" />
		</div>
	</div>
	<div class="body">
	</div>
</div>
