<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" EnableSessionState="false" %>
<!DOCTYPE html>
<html>
	<head runat="server">
		<title>Test Page</title>
		<meta charset="utf-8" />
		<meta name="viewport" content="width=device-width,minimum-scale=1,initial-scale=1" />
	</head>
	<body>
		<h1>Pixel Demo Page</h1>
		<div id="cookies"></div>
	</body>
	<script>
		(function (w, d, undefined) {
			var img = d.createElement("img"),
				dr = d.referrer || "";
			dr = encodeURIComponent(dr);
			img.src = "/?dr=" + dr + "&z=" + Math.random();

			// output code
			var cookies = d.cookie.split(/\W*;\W*/),
				output  = d.querySelector("#cookies");
			output.textContent = JSON.stringify(cookies);
		})(window, document);
	</script>
</html>