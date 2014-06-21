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
	#objeditor {
		display: none;
	}

	/* For some reason these won't cascade */
	#objeditor td {
		color: #fff;
	}
</style>

<div id="objeditor" class="modalpopup">
	<form>
		<div class="title">
			<div class="left"></div>
			<div class="right">
				<input class="savebtn" type="button" value="Save" />
				<input class="cancelbtn" type="button" value="Cancel" />
			</div>
		</div>
		<input name="id" class="editid" type="hidden" value="" />
		<div class="body">
			<table border="0">
				<tr>
					<td>Name:</td><td><input name="name" class="editname" type="text" value="" /></td>
				</tr>
				<tr>
					<td>Path ID:</td><td><input name="pathid" class="editpath" type="text" value="" /></td>
				</tr>
				<tr>
					<td>Parent:</td><td><input name="parent" class="editparent" type="text" value="" /></td>
				</tr>
				<tr>
					<td colspan="2">Description:</td>
				</tr>
				<tr>
					<td colspan="2">
						<textarea name="desc" class="editdesc" cols="70" rows="20"></textarea>
					</td>
				</tr>
			</table>
		</div>
	</form>
</div>
