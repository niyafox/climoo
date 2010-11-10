<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<style type="text/css">
	/* Stay invisible even if CSS isn't loaded */
	#texteditor {
		display: none;
	}

	/* For some reason these won't cascade */
	#texteditor td {
		color: #fff;
	}
</style>

<div id="texteditor" class="modalpopup">
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
			<textarea name="text" class="edittext" cols="70" rows="20"></textarea>
		</div>
	</form>
</div>
