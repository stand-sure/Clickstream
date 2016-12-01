namespace Clickstream.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
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
      return View ();
    }
  }
}
