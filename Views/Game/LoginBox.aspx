<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
