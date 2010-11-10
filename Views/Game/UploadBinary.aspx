<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

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
