using System.Web.Mvc;
using System.Web.Routing;

namespace Clickstream
{
#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
	public class RouteConfig
#pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
