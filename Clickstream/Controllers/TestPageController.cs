namespace Clickstream.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Mvc;
  using SessionStateBehavior = System.Web.SessionState.SessionStateBehavior;

  /// <summary>
  /// Test page controller.
  /// </summary>
  [SessionState(SessionStateBehavior.Disabled)]
  public class TestPageController : Controller
  {
    /// <summary>
    /// Default page
    /// </summary>
    public ActionResult Index()
    {
      var cookies = this.HttpContext.Request.Cookies;
      var model = cookies.AllKeys.Select(name => new
      {
        name,
        value = cookies.Get(name).Value.ToString()
      });

      return View(model);
    }
  }
}
