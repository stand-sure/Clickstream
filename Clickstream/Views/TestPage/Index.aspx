<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<!DOCTYPE html>
<html>
  <head runat="server">
    <title>Test Page</title>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width,minimum-scale=1,initial-scale=1" />
    </head>
    <body>
      <h1>Pixel Demo Page</h1>
      <div>
        <h2>Cookie Values</h2>
        <% foreach(var item in (IEnumerable<dynamic>)this.Model) {%>
          <%: item.ToString() %><br/>
        <%} %>
      </div>
      <a href="/testpage">Link to self (this will set a referrer)</a>
      <script>
	    (function (w, undefined) {
	      "use strict";
	      var l = w.location;
	      if (l.hostname !== "www.localexample.com") {
		    w.location = "//www.localexample.com:" + l.port + l.pathname;
	      }
	    })(window);
        (function (d, undefined) {
	      "use strict";
          var img = d.createElement("img"),
              dr = d.referrer || "";
          dr = encodeURIComponent(dr);
          img.src = "/?dr=" + dr + "&z=" + Math.random();
        })(document);
      </script>
  </body>
</html>