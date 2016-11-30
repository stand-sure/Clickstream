namespace Clickstream.Tests
{
	using System;
	//	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Moq;
	using NUnit.Framework;
	using Pixel = Controllers.HomeController;

	[TestFixture]
	public class PixelControllerTestClass
	{
		Mock<HttpRequestBase> request;
		Mock<HttpResponseBase> response;
		Mock<HttpContextBase> context;

		RequestContext rc;

		void SetUpRequest(HttpCookieCollection cookies) { 
			request = new Mock<HttpRequestBase>(MockBehavior.Strict);

			request.SetupGet(req => req.Cookies).Returns(cookies);
			var uri = new Uri("https://www.example.com/pixel?dr=https%3A%2F%2Fwww.example.com");
			request.SetupGet(req => req.Url).Returns(uri);
			var referrer = new Uri("https://www.example.com/referrer");
			request.SetupGet(req => req.UrlReferrer).Returns(referrer);
			request.SetupGet(req => req.UserAgent).Returns("my browser");
		}

		void SetUpResponse(HttpCookieCollection cookies)
		{
			response = new Mock<HttpResponseBase>(MockBehavior.Strict);
			response.SetupGet(resp => resp.Cookies).Returns(cookies);
			response.Setup(resp =>
			   resp.SetCookie(It.IsAny<HttpCookie>()))
				.Callback<HttpCookie>((cookie) => cookies.Add(cookie));
		}

		void SetUpHttpContext()
		{
			context = new Mock<HttpContextBase>(MockBehavior.Strict);
			context.SetupGet(ctx => ctx.Request).Returns(request.Object);
			context.SetupGet(ctx => ctx.Response).Returns(response.Object);
		}

		[SetUp]
		public void Init()
		{
			var cookies = new HttpCookieCollection();
			SetUpRequest(cookies);
			SetUpResponse(cookies);
			SetUpHttpContext();

			rc = new RequestContext(context.Object, new RouteData());
		}

		[Test]
		public void PixelShouldReturnAFile()
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			var actual = controller.Index();
			Assert.IsInstanceOf<FileContentResult>(actual, "Expected a FileResult");
		}

		[Test]
		public void PixelShouldReturnPng()
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			var result = controller.Index() as FileContentResult;
			var actual = result.ContentType;
			var expected = "image/png";
			Assert.AreEqual(expected, actual, "Expected a PNG image");
		}

		[Test]
		public void PixelShouldReturnCookies()
		{

			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			controller.Index();
			var cookies = controller.Response.Cookies;
			Assert.IsNotNull(cookies, "No cookies");
		}

		HttpCookie GetCookie(string name)
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			controller.Index();
			var cookies = controller.Response.Cookies;
			return cookies.Get(name);
		}

		[Test]
		public void PixelShouldSetCidCookie()
		{
			var actual = GetCookie("cid");
			Assert.IsNotNull(actual);
		}

		[Test]
		public void PixelShouldSetSidCookie()
		{
			var actual = GetCookie("sid");
			Assert.IsNotNull(actual);
		}

		[Test]
		public void PixelShouldSetScCookie()
		{
			var actual = GetCookie("sc");
			Assert.IsNotNull(actual);
		}

		[Test]
		public void PixelShouldSetSeqCookie()
		{
			var actual = GetCookie("seq");
			Assert.IsNotNull(actual);
		}

		[Test]
		public void PixelShouldIncrementSequenceCookie()
		{
			const string name = "seq";
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			controller.Index();
			var cookies = controller.Response.Cookies;
			var cookie = cookies.Get(name);
			var seq = int.Parse(cookie.Value ?? "0");
			var expected = seq + 1;

			controller.ControllerContext
				.HttpContext
				.Request
				.Cookies
				.Set(cookie);
			controller.Index();
			cookies = controller.Response.Cookies;
			var actual = int.Parse(cookies.Get(name).Value);
			Assert.AreEqual(expected, actual, "Sequence should increment");
		}

		[Test]
		public void PixelShouldIncrementSessionCountIfNoSid()
		{
			const string name = "sc";
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);
			controller.ControllerContext
				.HttpContext
				.Request
				.Cookies
				.Set(new HttpCookie(name, "1"));

			controller.Index();
			var cookies = controller.Response.Cookies;
			var cookie = cookies.Get(name) ?? new HttpCookie("f00", "0");
			var actual = int.Parse(cookie.Value);
			var expected = 2;
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void PixelShouldNotIncrementSessionCountIfSidSet()
		{
			const string name = "sc";
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);
			controller.ControllerContext
				.HttpContext
				.Request
				.Cookies
				.Set(new HttpCookie(name, "1"));
			controller.ControllerContext
				.HttpContext
				.Request
				.Cookies
				.Set(new HttpCookie("sid", "1"));

			controller.Index();
			var cookies = controller.Response.Cookies;
			var cookie = cookies.Get(name) ?? new HttpCookie("f00", "0");
			var actual = int.Parse(cookie.Value);
			var expected = 1;
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void PixelShouldNotSetNewCidIfAlreadySet()
		{
			const string name = "cid";
			string expected = (new Random()).Next().ToString();
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);
			controller.ControllerContext
				.HttpContext
				.Request
				.Cookies
				.Set(new HttpCookie(name, expected));

			controller.Index();
			var cookies = controller.Response.Cookies;
			var cookie = cookies.Get(name) ?? new HttpCookie("f00", "abcde");
			var actual = cookie.Value;
			Assert.AreEqual(expected, actual);
		}

		bool CookieValueShouldBeAGuid(string name)
		{
			HttpCookie cookie = GetCookie(name) ?? new HttpCookie("f00");
			string text = cookie.Value;
			Guid guid;
			bool retVal = Guid.TryParse(text, out guid);
			return retVal;
		}

		[Test]
		public void SidShouldBeGuid()
		{
			bool actual = CookieValueShouldBeAGuid("sid");
			Assert.IsTrue(actual, "sid should be a GUID");
		}

		[Test]
		public void CidShouldBeGuid()
		{
			bool actual = CookieValueShouldBeAGuid("cid");
			Assert.IsTrue(actual, "cid should be a GUID");
		}

		bool CookieValueShouldBeAnInt(string name)
		{
			HttpCookie cookie = GetCookie(name) ?? new HttpCookie("foo");
			string text = cookie.Value;
			int val;
			bool retVal = int.TryParse(text, out val);
			return retVal;
		}

		[Test]
		public void SeqShouldBeAnInteger()
		{
			bool actual = CookieValueShouldBeAnInt("seq");
			Assert.IsTrue(actual);
		}

		[Test]
		public void ScShouldBeAnInteger()
		{
			bool actual = CookieValueShouldBeAnInt("sc");
			Assert.IsTrue(actual);
		}

		bool CookieIsSessionOnly(string cookieName)
		{
			HttpCookie cookie = GetCookie(cookieName);
			bool retVal = cookie.Expires == default(DateTime);
			return retVal;
		}

		[Test]
		public void CidShouldBeAPermanentCookie()
		{
			Assert.False(CookieIsSessionOnly("cid"));
		}

		[Test]
		public void SidShouldBeATemporaryCookie()
		{
			Assert.True(CookieIsSessionOnly("sid"));
		}

		[Test]
		public void ScShouldBeAPermanentCookie()
		{
			Assert.False(CookieIsSessionOnly("sc"));
		}

		[Test]
		public void SeqShouldBeATemporaryCookie()
		{
			Assert.True(CookieIsSessionOnly("seq"));
		}

		[Test]
		public void IndexShouldCallLogValues()
		{
			var mock = new Mock<Pixel> { CallBase = true };
			mock.Object.ControllerContext = new ControllerContext(rc, mock.Object);
			mock.Object.Index();
			mock.Verify(foo => foo.LogValues(It.IsAny<HttpContextBase>()));
		}

		[Test]
		public void IndexShouldCallSerializeValues()
		{
			var mock = new Mock<Pixel> { CallBase = true };
			mock.Object.ControllerContext = new ControllerContext(rc, mock.Object);
			mock.Object.Index();
			mock.Verify(foo => foo.SerializeValues(It.IsAny<HttpContextBase>()));
		}

		bool JsonContainsKey(string name)
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);
			controller.Index();
			string json = controller.SerializeValues(controller.HttpContext);
			bool retVal = json.Contains("\"" + name + "\":");
			return retVal;
		}

		[Test]
		public void SerializedValuesShouldIncludeCid()
		{
			const string name = "cid";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludeSid()
		{
			const string name = "sid";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludeSeq()
		{
			const string name = "seq";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludeSc()
		{
			const string name = "sc";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludeTime()
		{
			const string name = "time";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludePage()
		{
			const string name = "page";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludePreviousPage()
		{
			const string name = "previousPage";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void SerializedValuesShouldIncludeUserAgent()
		{
			const string name = "userAgent";
			Assert.IsTrue(JsonContainsKey(name));
		}

		[Test]
		public void PixelShouldOnlyAcceptHttpGet()
		{
			var method = typeof(Pixel).GetMethod("Index");
			var attribute = method.GetCustomAttributes(typeof(HttpGetAttribute), false);
			Assert.IsNotNull(attribute);
		}

		[Test]
		public void ResponseShouldNotBeCached() 
		{
			var method = typeof(Pixel).GetMethod("Index");
			OutputCacheAttribute attribute = method.GetCustomAttributes(typeof(OutputCacheAttribute), false)[0] 
			                                       as OutputCacheAttribute;
			Assert.IsNotNull(attribute);
			Assert.LessOrEqual(attribute.Duration, 0);
		}

	}
}
