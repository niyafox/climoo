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
	#loginbox {
		display: none;
	}

	/* For some reason these won't cascade */
	#loginbox td {
		color: #fff;
	}
</style>

<div id="loginbox" class="modalpopup">
	<form>
		<div class="body">
			<table border="0">
				<tr>
					<td>Login:</td><td><input name="login" class="login" type="text" value="" /></td>
				</tr>
				<tr>
					<td>Password:</td><td><input name="password" class="password" type="password" value="" /></td>
				</tr>
				<tr>
					<td></td>
					<td>
						<input class="loginbtn" type="button" value="Login" />
						<input class="cancelbtn" type="button" value="Cancel" />
					</td>
				</tr>
			</table>
		</div>
	</form>
</div>
