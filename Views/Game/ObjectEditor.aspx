<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
