//
//  HomeControllerTest.cs
//
//  Author:
//       C. J. Anderson <chris@standsure.io>
//
//  Copyright (c) 2016 2016
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace Clickstream.Tests
{
  using System;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Routing;
  using Moq;
  using NUnit.Framework;
  using Pixel = Controllers.HomeController;

  /// <summary>
  /// Pixel controller test class.
  /// </summary>
  [TestFixture]
  public class PixelControllerTestClass : AssertionHelper
  {

    #region SetUp

    Mock<HttpRequestBase> request;
    Mock<HttpResponseBase> response;
    Mock<HttpContextBase> context;

    RequestContext rc;

    Pixel controller;

    /// <summary>
    /// Sets up request.
    /// </summary>
    /// <param name="cookies">Cookies.</param>
		void SetUpRequest(HttpCookieCollection cookies)
    {
      request = new Mock<HttpRequestBase>(MockBehavior.Strict);

      request.SetupGet(req => req.Cookies).Returns(cookies);
      var uri = new Uri("https://www.example.com/pixel?" +
                        "&utm_source=google" +
                        "&utm_medium=cpc" +
                        "&utm_term=clickstream" +
                        "&utm_campaign=analytics" +
                        "&dr=https%3A%2F%2Fwww.example.com");
      request.SetupGet(req => req.Url).Returns(uri);
      var referrer = new Uri("https://www.example.com/referrer?" + 
                        "&utm_source=google" +
                        "&utm_medium=cpc" +
                        "&utm_term=clickstream" +
                        "&utm_campaign=analytics" +
                        "&dr=https%3A%2F%2Fwww.example.com");
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
    /// Sets up the Pixel controller.
    /// </summary>
    void SetUpController()
    {
      controller = new Pixel();
      controller.ControllerContext = new ControllerContext(rc, controller);
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
      SetUpController();
    }

    #endregion

    #region Return Value Tests

    /// <summary>
    /// A test to verfiy that a file is returned.
    /// </summary>
		[Test]
    public void PixelShouldReturnAFile()
    {
      var actual = controller.Index();
      Assert.IsInstanceOf<FileContentResult>(actual, "Expected a FileResult");
    }

    /// <summary>
    /// A test to verify that the MIME type returned is "image/png".
    /// </summary>
		[Test]
    public void PixelShouldReturnPng()
    {
      var result = controller.Index() as FileContentResult;
      Assert.IsNotNull(result);
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
      Expect(GetCookie("cid"), Is.Not.Null);
    }

    /// <summary>
    /// A test to verify that a cookie with name "sid" is set.
    /// </summary>
		[Test]
    public void PixelShouldSetSidCookie()
    {
      Expect(GetCookie("sid"), Is.Not.Null);
    }

    /// <summary>
    /// A test to verify that a cookie with name "sc" is set.
    /// </summary>
		[Test]
    public void PixelShouldSetScCookie()
    {
      Expect(GetCookie("sc"), Is.Not.Null);
    }

    /// <summary>
    /// A test to verify that a cookie with name "seq" is set.
    /// </summary>
		[Test]
    public void PixelShouldSetSeqCookie()
    {
      Expect(GetCookie("seq"), Is.Not.Null);
    }

    /// <summary>
    /// Verifies that an attempt to get cookie with empty name should 
    /// fail gracefully.
    /// </summary>
    [Test]
    public void AttemptsToGetCookieWithEmptyNameShouldFailGracefully()
    {
      Assert.DoesNotThrow(() => GetCookie(string.Empty));
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

      Expect(int.Parse(cookies.Get(name).Value), Is.EqualTo(expected));
    }

    /// <summary>
    /// A test to verify that the session count (sc) cookie is incremented if there is no session ID (sid).
    /// </summary>
		[Test]
    public void PixelShouldIncrementSessionCountIfNoSid()
    {
      const string name = "sc";
      controller.ControllerContext
        .HttpContext
        .Request
        .Cookies
        .Set(new HttpCookie(name, "1"));

      controller.Index();
      var cookies = controller.Response.Cookies;
      var cookie = cookies.Get(name) ?? new HttpCookie("f00", "0");

      Expect(int.Parse(cookie.Value), Is.EqualTo(2));
    }

    /// <summary>
    /// A test to verfiy that the session count cookie (sc) is not increment if there is a session id (sid).
    /// </summary>
		[Test]
    public void PixelShouldNotIncrementSessionCountIfSidSet()
    {
      const string name = "sc";
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

      Expect(int.Parse(cookie.Value), Is.EqualTo(1));
    }

    /// <summary>
    /// A test to verify that the client id (cid) is not changed if it has already been set.
    /// </summary>
		[Test]
    public void PixelShouldNotSetNewCidIfAlreadySet()
    {
      const string name = "cid";
      string expected = (new Random()).Next().ToString();
      controller.ControllerContext
        .HttpContext
        .Request
        .Cookies
        .Set(new HttpCookie(name, expected));

      controller.Index();
      var cookies = controller.Response.Cookies;
      var cookie = cookies.Get(name) ?? new HttpCookie("f00", "abcde");

      Expect(cookie.Value, Is.EqualTo(expected));
    }

    /// <summary>
    /// A test to verify that the session id (sid) is not changed if it has already been set.
    /// </summary>
    [Test]
    public void PixelShouldNotSetNewSidIfAlreadySet()
    {
      const string name = "sid";
      string expected = (new Random()).Next().ToString();
      controller.ControllerContext
        .HttpContext
        .Request
        .Cookies
        .Set(new HttpCookie(name, expected));

      controller.Index();
      var cookies = controller.Response.Cookies;
      var cookie = cookies.Get(name) ?? new HttpCookie("f00", "abcde");

      Expect(cookie.Value, Is.EqualTo(expected));
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
      Expect(CookieValueShouldBeAGuid("sid"), "Expected a GUID");
    }

    /// <summary>
    /// A test to confirm that the cid value is a GUID.
    /// </summary>
		[Test]
    public void CidShouldBeGuid()
    {
      Expect(CookieValueShouldBeAGuid("cid"), "cid should be a GUID");
    }

    /// <summary>
    /// A test to verify that the seq cookie value is an int.
    /// </summary>
		[Test]
    public void SeqShouldBeAnInteger()
    {
      Expect(CookieValueShouldBeAnInt("seq"));
    }

    /// <summary>
    /// A test to verify that the sc cookie value is an int.
    /// </summary>
		[Test]
    public void ScShouldBeAnInteger()
    {
      Expect(CookieValueShouldBeAnInt("sc"));
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
      Expect(CookieIsSessionOnly("cid"), Is.False);
    }

    /// <summary>
    /// A test to verify that the sid cookie is session-only.
    /// </summary>
		[Test]
    public void SidShouldBeATemporaryCookie()
    {
      Expect(CookieIsSessionOnly("sid"), Is.True);
    }

    /// <summary>
    /// A test to verify that the sc cookie is permanent.
    /// </summary>
		[Test]
    public void ScShouldBeAPermanentCookie()
    {
      Expect(CookieIsSessionOnly("sc"), Is.False);
    }

    /// <summary>
    /// A test to verify that the seq cookie is session-only.
    /// </summary>
		[Test]
    public void SeqShouldBeATemporaryCookie()
    {
      Expect(CookieIsSessionOnly("seq"), Is.True);
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

    #region serialization tests

    /// <summary>
    /// A helper method to verfify that the JSON contains the key name.
    /// </summary>
    /// <returns><c>true</c>, if contains key was found in the JSON, <c>false</c> otherwise.</returns>
    /// <param name="name">The key name.</param>
		bool JsonContainsKey(string name)
    {
      controller.Index();
      string json = controller.SerializeValues(controller.HttpContext);
      bool retVal = json.Contains("\"" + name + "\":");
      return retVal;
    }

    /// <summary>
    /// A helper method to verify that the JSON contains the key-value pair.
    /// </summary>
    /// <returns><c>true</c>, if contains value was found, <c>false</c> otherwise.</returns>
    /// <param name="name">The key name.</param>
    /// <param name="value">The value.</param>
    bool JsonContainsValue(string name, string value)
    {
      controller.Index();
      string json = controller.SerializeValues(controller.HttpContext);
      bool retVal = json.Contains("\"" + name + "\":\"" + value + "\"");
      return retVal;
    }

    /// <summary>
    /// A test to verify that the client ID (cid) is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeCid()
    {
      Expect(JsonContainsKey("cid"), Is.True);
    }

    /// <summary>
    /// A test to verify that the session ID (sid) is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeSid()
    {
      Expect(JsonContainsKey("sid"), Is.True);
    }

    /// <summary>
    /// A test to verify that the sequence count (seq) is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeSeq()
    {
      Expect(JsonContainsKey("seq"), Is.True);
    }

    /// <summary>
    /// A test to verify that session count (sc) is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeSc()
    {
      Expect(JsonContainsKey("sc"), Is.True);
    }

    /// <summary>
    /// A test to verify that call time is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeTime()
    {
      Expect(JsonContainsKey("time"), Is.True);
    }

    /// <summary>
    /// A test to verify that the calling page is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludePage()
    {
      Expect(JsonContainsKey("page"), Is.True);
    }

    /// <summary>
    /// A test to verify that the previous page is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludePreviousPage()
    {
      Expect(JsonContainsKey("previousPage"), Is.True);
    }

    /// <summary>
    /// A test to verify that the previous page is properly set.
    /// </summary>
    [Test]
    public void PreviousPageIsProperlySet()
    {
      Expect(JsonContainsValue("previousPage", "https://www.example.com"), Is.True);
    }

    /// <summary>
    /// A test to verify that the page is properly set
    /// </summary>
    [Test]
    public void PageIsProperlySet()
    {
      string value = "https://www.example.com/referrer?" + 
        "&utm_source=google" +
        "&utm_medium=cpc" +
        "&utm_term=clickstream" +
        "&utm_campaign=analytics" +
        "&dr=https:%2F%2Fwww.example.com";
      Expect(JsonContainsValue("page", value), Is.True);
    }

    /// <summary>
    /// A test to verify that UserAgent is serialized.
    /// </summary>
		[Test]
    public void SerializedValuesShouldIncludeUserAgent()
    {
      Expect(JsonContainsKey("userAgent"), Is.True);
    }

    #endregion

    #region Index attribute tests for HttpGet and OutputCache

    /// <summary>
    /// A test to verify that Index only accepts HTTP GET.
    /// </summary>
		[Test]
    public void PixelShouldOnlyAcceptHttpGet()
    {
      var method = typeof(Pixel).GetMethod("Index");
      var attribute = method.GetCustomAttributes(typeof(HttpGetAttribute), false);
      Expect(attribute, Is.Not.Null.Or.Empty);
    }

    /// <summary>
    /// A test to verify that the Response is not cached.
    /// </summary>
		[Test]
    public void ResponseShouldNotBeCached()
    {
      var method = typeof(Pixel).GetMethod("Index");
      var attribute = method.GetCustomAttributes(typeof(OutputCacheAttribute), false)[0]
                                             as OutputCacheAttribute;
      Expect(attribute, Is.Not.Null);
      Expect(attribute.Duration, Is.LessThanOrEqualTo(0));
    }

    #endregion

    #region cookie attribute tests

    /// <summary>
    /// Tests whether all of the cookies set by the Pixel are HTTP-only.
    /// </summary>
    [Test]
    public void AllCookiesShouldBeHTTPOnly()
    {
      controller.Index();
      var cookies = controller.Response.Cookies.AllKeys
                              .Select(key => GetCookie(key))
                              .ToArray();
      cookies.ForEach(cookie =>
      {
        Expect(cookie.HttpOnly, Is.True);
      });

    }

    /// <summary>
    /// Tests whether all of the cookies set by the Pixel have a domain.
    /// </summary>
    [Test]
    public void AllCookiesShouldHaveDomainSet()
    {
      controller.Index();
      var cookies = controller.Response.Cookies.AllKeys
                              .Select(key => GetCookie(key))
                              .ToArray();
      cookies.ForEach(cookie =>
      {
        Expect(cookie.Domain, Is.Not.Null.Or.Empty);
      });
    }

    /// <summary>
    /// A test to verify that cookies are marked secure.
    /// </summary>
    [Test]
    public void CookiesShouldBeSecure() {
      controller.Index();

      var cookies = controller.Response.Cookies.AllKeys
                              .Select(key => GetCookie(key))
                              .ToArray();
      cookies.ForEach(cookie =>
      {
        Expect(cookie.Secure, Is.True);
      });
    }

    #endregion

  }
}
