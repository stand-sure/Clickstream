using System.Web.Mvc;
using System.Web.Routing;

namespace Clickstream
{
  #pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.
  #pragma warning disable CS1591
  public class RouteConfig
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
  #pragma warning restore RECS0014 // If all fields, properties and methods members are static, the class can be made static.
  #pragma warning restore CS1591
}
