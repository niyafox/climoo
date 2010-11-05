<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<style type="text/css">
	/* For some reason these won't cascade */
	#objeditor td {
		color: #fff;
	}
</style>

<div id="objeditor" class="modalpopup">
	<div class="title">
		<div class="left"></div>
		<div class="right">
			<input class="savebtn" type="button" value="Save" />
			<input class="cancelbtn" type="button" value="Cancel" />
		</div>
	</div>
	<div class="body">
		<table border="0" width="100%">
			<tr>
				<td>Name:</td><td><input class="editname" type="text" value="" /></td>
			</tr>
			<tr>
				<td>Path ID:</td><td><input class="editpath" type="text" value="" /></td>
			</tr>
			<tr>
				<td>Parent:</td><td><input class="editparent" type="text" value="" /></td>
			</tr>
			<tr>
				<td colspan="2">Description:</td>
			</tr>
			<tr>
				<td colspan="2">
					<textarea class="editdesc" cols="70" rows="20"></textarea>
				</td>
			</tr>
		</table>
	</div>
</div>
