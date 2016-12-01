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

    #region SetUp

		Mock<HttpRequestBase> request;
		Mock<HttpResponseBase> response;
		Mock<HttpContextBase> context;

		RequestContext rc;

    /// <summary>
    /// Sets up request.
    /// </summary>
    /// <param name="cookies">Cookies.</param>
		void SetUpRequest(HttpCookieCollection cookies) { 
			request = new Mock<HttpRequestBase>(MockBehavior.Strict);

			request.SetupGet(req => req.Cookies).Returns(cookies);
			var uri = new Uri("https://www.example.com/pixel?dr=https%3A%2F%2Fwww.example.com");
			request.SetupGet(req => req.Url).Returns(uri);
			var referrer = new Uri("https://www.example.com/referrer");
			request.SetupGet(req => req.UrlReferrer).Returns(referrer);
			request.SetupGet(req => req.UserAgent).Returns("my browser");
		}

    /// <summary>
    /// Sets up response.
    /// </summary>
    /// <param name="cookies">Cookies.</param>
		void SetUpResponse(HttpCookieCollection cookies)
		{
			response = new Mock<HttpResponseBase>(MockBehavior.Strict);
			response.SetupGet(resp => resp.Cookies).Returns(cookies);
			response.Setup(resp =>
			   resp.SetCookie(It.IsAny<HttpCookie>()))
				.Callback<HttpCookie>((cookie) => cookies.Add(cookie));
		}

    /// <summary>
    /// Sets up http context.
    /// </summary>
		void SetUpHttpContext()
		{
			context = new Mock<HttpContextBase>(MockBehavior.Strict);
			context.SetupGet(ctx => ctx.Request).Returns(request.Object);
			context.SetupGet(ctx => ctx.Response).Returns(response.Object);
		}

    /// <summary>
    /// Sets up request context.
    /// </summary>
		void SetUpRequestContext()
		{
			rc = new RequestContext(context.Object, new RouteData());
		}

    /// <summary>
    /// A method called just before each test method.
    /// Initializes contexts.
    /// </summary>
		[SetUp]
		public void Init()
		{
			var cookies = new HttpCookieCollection();
			SetUpRequest(cookies);
			SetUpResponse(cookies);
			SetUpHttpContext();
			SetUpRequestContext();
		}

    #endregion

    #region Return Value Tests

    /// <summary>
    /// A test to verfiy that a file is returned.
    /// </summary>
		[Test]
		public void PixelShouldReturnAFile()
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			var actual = controller.Index();
			Assert.IsInstanceOf<FileContentResult>(actual, "Expected a FileResult");
		}

    /// <summary>
    /// A test to verify that the MIME type returned is "image/png".
    /// </summary>
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

    /// <summary>
    /// A test to verify that cookies are returned.
    /// </summary>
		[Test]
		public void PixelShouldReturnCookies()
		{

			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			controller.Index();
			var cookies = controller.Response.Cookies;
			Assert.IsNotNull(cookies, "No cookies");
		}

    #endregion

    /// <summary>
    /// Gets an HttpCookie from the response.
    /// </summary>
    /// <returns>The cookie.</returns>
    /// <param name="name">The Name of the cookie.</param>
		HttpCookie GetCookie(string name)
		{
			var controller = new Pixel();
			controller.ControllerContext = new ControllerContext(rc, controller);

			controller.Index();
			var cookies = controller.Response.Cookies;
			return cookies.Get(name);
		}

    #region Tests to verify that specifc cookies are set

    /// <summary>
    /// A test to verify that a cookie with name "cid" is set.
    /// </summary>
		[Test]
		public void PixelShouldSetCidCookie()
		{
			var actual = GetCookie("cid");
			Assert.IsNotNull(actual);
		}

    /// <summary>
    /// A test to verify that a cookie with name "sid" is set.
    /// </summary>
		[Test]
		public void PixelShouldSetSidCookie()
		{
			var actual = GetCookie("sid");
			Assert.IsNotNull(actual);
		}

    /// <summary>
    /// A test to verify that a cookie with name "sc" is set.
    /// </summary>
		[Test]
		public void PixelShouldSetScCookie()
		{
			var actual = GetCookie("sc");
			Assert.IsNotNull(actual);
		}

    /// <summary>
    /// A test to verify that a cookie with name "seq" is set.
    /// </summary>
		[Test]
		public void PixelShouldSetSeqCookie()
		{
			var actual = GetCookie("seq");
			Assert.IsNotNull(actual);
		}

    #endregion

    #region Tests to verify cookie value update behavior.

    /// <summary>
    /// A test to verify the the sequence cookie (seq) is incremented.
    /// </summary>
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

    /// <summary>
    /// A test to verify that the session count (sc) cookie is incremented if there is no session ID (sid).
    /// </summary>
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

    /// <summary>
    /// A test to verfiy that the session count cookie (sc) is not increment if there is a session id (sid).
    /// </summary>
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

    /// <summary>
    /// A test to verify that the client id (cid) is not changed if it has already been set.
    /// </summary>
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

    #endregion

    #region cookie value type tests

    /// <summary>
    /// Verifies that the cookie value is a GUID.
    /// </summary>
    /// <returns><c>true</c>, if value should be was a GUID, <c>false</c> otherwise.</returns>
    /// <param name="name">The cookie name.</param>
		bool CookieValueShouldBeAGuid(string name)
		{
			HttpCookie cookie = GetCookie(name) ?? new HttpCookie("f00");
			string text = cookie.Value;
			Guid guid;
			bool retVal = Guid.TryParse(text, out guid);
			return retVal;
		}

    /// <summary>
    /// Verifies that the cookie value is an int.
    /// </summary>
    /// <returns><c>true</c>, if the value is an int, <c>false</c> otherwise.</returns>
    /// <param name="name">Name.</param>
    bool CookieValueShouldBeAnInt(string name)
    {
      HttpCookie cookie = GetCookie(name) ?? new HttpCookie("foo");
      string text = cookie.Value;
      int val;
      bool retVal = int.TryParse(text, out val);
      return retVal;
    }

    /// <summary>
    /// A test to confirm that the sid value is a GUID.
    /// </summary>
		[Test]
		public void SidShouldBeGuid()
		{
			bool actual = CookieValueShouldBeAGuid("sid");
			Assert.IsTrue(actual, "sid should be a GUID");
		}

    /// <summary>
    /// A test to confirm that the cid value is a GUID.
    /// </summary>
		[Test]
		public void CidShouldBeGuid()
		{
			bool actual = CookieValueShouldBeAGuid("cid");
			Assert.IsTrue(actual, "cid should be a GUID");
		}

    /// <summary>
    /// A test to verify that the seq cookie value is an int.
    /// </summary>
		[Test]
		public void SeqShouldBeAnInteger()
		{
			bool actual = CookieValueShouldBeAnInt("seq");
			Assert.IsTrue(actual);
		}

    /// <summary>
    /// A test to verify that the sc cookie value is an int.
    /// </summary>
		[Test]
		public void ScShouldBeAnInteger()
		{
			bool actual = CookieValueShouldBeAnInt("sc");
			Assert.IsTrue(actual);
		}

    #endregion

    #region Cookie persistense tests

    /// <summary>
    /// Verifies that a cookie is session only.
    /// </summary>
    /// <returns><c>true</c>, if is the cookie is session only, <c>false</c> otherwise.</returns>
    /// <param name="cookieName">Cookie name.</param>
		bool CookieIsSessionOnly(string cookieName)
		{
			HttpCookie cookie = GetCookie(cookieName);
			bool retVal = cookie.Expires == default(DateTime);
			return retVal;
		}

    /// <summary>
    /// A test to verify that the cid cookie is permanent.
    /// </summary>
		[Test]
		public void CidShouldBeAPermanentCookie()
		{
			Assert.False(CookieIsSessionOnly("cid"));
		}

    /// <summary>
    /// A test to verify that the sid cookie is session-only.
    /// </summary>
		[Test]
		public void SidShouldBeATemporaryCookie()
		{
			Assert.True(CookieIsSessionOnly("sid"));
		}

    /// <summary>
    /// A test to verify that the sc cookie is permanent.
    /// </summary>
		[Test]
		public void ScShouldBeAPermanentCookie()
		{
			Assert.False(CookieIsSessionOnly("sc"));
		}

    /// <summary>
    /// A test to verify that the seq cookie is session-only.
    /// </summary>
		[Test]
		public void SeqShouldBeATemporaryCookie()
		{
			Assert.True(CookieIsSessionOnly("seq"));
		}

    #endregion

    #region Tests to verfiy that certain helper methods are called

    /// <summary>
    /// A test to verify that LogValues is called.
    /// </summary>
		[Test]
		public void IndexShouldCallLogValues()
		{
			var mock = new Mock<Pixel> { CallBase = true };
			mock.Object.ControllerContext = new ControllerContext(rc, mock.Object);
			mock.Object.Index();
			mock.Verify(foo => foo.LogValues(It.IsAny<HttpContextBase>()));
		}

    /// <summary>
    /// A test to verfiy that Serialize Values is called.
    /// </summary>
		[Test]
		public void IndexShouldCallSerializeValues()
		{
			var mock = new Mock<Pixel> { CallBase = true };
			mock.Object.ControllerContext = new ControllerContext(rc, mock.Object);
			mock.Object.Index();
			mock.Verify(foo => foo.SerializeValues(It.IsAny<HttpContextBase>()));
		}

    #endregion

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
