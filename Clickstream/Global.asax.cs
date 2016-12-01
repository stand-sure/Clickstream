using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Clickstream
{
  #pragma warning disable CS1591 // XML comments
	public class Global : HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
  #pragma warning restore CS1591
}
