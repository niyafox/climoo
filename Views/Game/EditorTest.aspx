<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>EditorTest</title>
	<style type="text/css">
		body {
			margin: 0px 0px 0px 0px;
			padding: 0px 0px 0px 0px;
		}
		body > div {
			margin: 0px 0px 0px 0px;
			padding: 2px 2px 2px 2px;
			background-color: #002;
			color: #888;
			font-weight: bold;
		}
	</style>
</head>
<body>
    <div>
		<%
			for (int i=0; i<5000; ++i)
				Response.Write("quux ");
		%>
    </div>
</body>
</html>
