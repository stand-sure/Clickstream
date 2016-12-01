namespace Clickstream.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;

    /// <summary>
    /// Test page controller.
    /// </summary>
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
