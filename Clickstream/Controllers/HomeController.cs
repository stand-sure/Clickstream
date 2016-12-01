namespace Clickstream.Controllers
{
	using System;
	using System.Web;
	using System.Web.Mvc;

  /// <summary>
  /// Home controller.
  /// </summary>
	public class HomeController : Controller
	{
    static string DOMAIN = "localexample.com"; // TODO: change this to your root domain

    #region cookie helpers

    /// <summary>
    /// Sets the cookie.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="permanent">If set to <c>true</c> the the cookie is made permanent.</param>
		static void SetCookie(HttpContextBase context, string name, string value = "", bool permanent = false)
		{
			var cookie = context.Request.Cookies.Get(name) ?? new HttpCookie(name);
			cookie.Value = value;
			if (permanent)
			{
				cookie.Expires = new DateTime(2099, 12, 31, 23, 59, 59);
			}

      cookie.HttpOnly = true;
      cookie.Domain = DOMAIN;
			context.Response.SetCookie(cookie);
		}

    /// <summary>
    /// Gets the cookie value.
    /// </summary>
    /// <returns>The cookie value.</returns>
    /// <param name="context">The HttpContext.</param>
    /// <param name="name">The cookie name.</param>
		static string GetCookieValue(HttpContextBase context, string name)
		{
			var cookies = context.Request.Cookies;
			return (cookies.Get(name) ?? new HttpCookie("foo", string.Empty)).Value;
		}

    /// <summary>
    /// Updates the cookies.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
		static void UpdateCookies(HttpContextBase context)
		{
			string seqText = GetCookieValue(context, "seq");
			if (string.IsNullOrEmpty(seqText))
			{
				seqText = "0";
			}
			var seq = int.Parse(seqText);
			SetCookie(context, "seq", (seq + 1).ToString());

			var sid = GetCookieValue(context, "sid");
			string scText = GetCookieValue(context, "sc");
			var sc = int.Parse(string.IsNullOrEmpty(scText) ? "0" : scText);
			if (string.IsNullOrEmpty(sid))
			{
				sc += 1;
			}

			SetCookie(context, "sc", sc.ToString(), permanent: true);

			var cid = GetCookieValue(context, "cid");
			SetCookie(context, "cid", string.IsNullOrEmpty(cid) ? Guid.NewGuid().ToString() : cid, permanent: true);

			SetCookie(context, "sid", Guid.NewGuid().ToString());
		}

    #endregion

    #region value serialization and logging helpers

    /// <summary>
    /// Serializes the cookie values to JSON.
    /// </summary>
    /// <returns>The a JSON string.</returns>
    /// <param name="context">The HttpContext.</param>
		public virtual string SerializeValues(HttpContextBase context)
		{
			var cookies = context.Response.Cookies;
			var javaScriptSerializer = new
				System.Web.Script.Serialization.JavaScriptSerializer();
			var urlReferrer = context.Request.UrlReferrer;

			string page = urlReferrer != null ? urlReferrer.ToString() : string.Empty;
			var queryValues = HttpUtility.ParseQueryString(context.Request.Url.Query);
			string previousPage = queryValues["dr"];
			string userAgent = context.Request.UserAgent;
			string time = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

			var obj = new
			{
				cid = cookies.Get("cid").Value,
				sid = cookies.Get("sid").Value,
				sc = cookies.Get("sc").Value,
				seq = cookies.Get("seq").Value,
				page,
				previousPage,
				userAgent,
				time
			};

			string jsonString = javaScriptSerializer.Serialize(obj);
			return jsonString;
		}

    /// <summary>
    /// Logs the values.
    /// </summary>
    /// <returns>The values.</returns>
    /// <param name="context">Context.</param>
		public virtual string LogValues(HttpContextBase context)
		{
			var values = SerializeValues(HttpContext);

      // TODO: implemement logging as suits your environment.

			return values;
		}

    #endregion

    /// <summary>
    /// Updates cookies, logs the values and returns an image.
    /// This endpoint is intended to be called by JavaScript in the client HTML.
    /// </summary>
		[HttpGet]
		[OutputCache(Duration = -1)]
		public ActionResult Index()
		{
			byte[] bytes = new byte[1] { 0 };

			UpdateCookies(HttpContext);

			LogValues(HttpContext);
			return new FileContentResult(bytes, "image/png");
		}
	}
}
