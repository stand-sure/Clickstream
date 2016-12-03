namespace Clickstream.Controllers
{
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
      var cookies = HttpContext.Request.Cookies;
      var model = cookies.AllKeys.Select(name => new
      {
        name,
        value = cookies.Get(name).Value
      });

      return View(model);
    }
  }
}
