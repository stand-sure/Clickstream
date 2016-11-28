namespace Clickstream.Controllers
{
	using System;
	using System.Web;
	using System.Web.Mvc;

	public class HomeController : Controller
	{
		static void SetCookie(HttpContextBase context, string name, string value = "", bool permanent = false)
		{
			var cookie = context.Request.Cookies.Get(name) ?? new HttpCookie(name);
			cookie.Value = value;
			if (permanent)
			{
				cookie.Expires = new DateTime(2099, 12, 31, 23, 59, 59);
			}

			context.Response.SetCookie(cookie);
		}

		static string GetCookieValue(HttpContextBase context, string name)
		{
			var cookies = context.Request.Cookies;
			return (cookies.Get(name) ?? new HttpCookie("foo", string.Empty)).Value;
		}

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

		public virtual string SerializeValues(HttpContextBase context)
		{
			var cookies = context.Response.Cookies;
			var javaScriptSerializer = new
				System.Web.Script.Serialization.JavaScriptSerializer();
			string page = context.Request.UrlReferrer.ToString();
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

		public virtual string LogValues(HttpContextBase context)
		{
			var values = SerializeValues(HttpContext);
			return values;
		}

		public ActionResult Index()
		{
			byte[] bytes = new byte[0];

			UpdateCookies(HttpContext);

			LogValues(HttpContext);
			return new FileContentResult(bytes, "image/png");
		}
	}
}
