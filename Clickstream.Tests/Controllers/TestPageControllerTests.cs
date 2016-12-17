//
//  TestPageControllerTests.cs
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
  using System.Collections.Generic;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Routing;
  using Moq;
  using NUnit.Framework;
  using TestPage = Controllers.TestPageController;

  /// <summary>
  /// Test page controller tests.
  /// </summary>
  [TestFixture]
  public class TestPageControllerTests : AssertionHelper
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
    void SetUpRequest(HttpCookieCollection cookies)
    {
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

    /// <summary>
    /// A test to verify that Index returns a web page.
    /// </summary>
    [Test]
    public void IndexShouldReturnAWebPage()
    {
      var controller = new TestPage();
      controller.ControllerContext = new ControllerContext(rc, controller);

      Expect(controller.Index(), Is.InstanceOf<ViewResult>());
    }

    /// <summary>
    /// Indexs the should use a model that is IEnumerable.
    /// </summary>
    [Test]
    public void IndexShouldUseAModelThatIsIEnumerable() 
    {
      var controller = new TestPage();
      controller.ControllerContext = new ControllerContext(rc, controller);

      var result = (controller.Index() as ViewResult).Model;

      Expect(result, Is.Not.Null);
      Expect(result, Is.InstanceOf<IEnumerable<dynamic>>());
    }

    /// <summary>
    /// A test to verify that the test page accesses Cookies
    /// </summary>
    [Test]
    public void IndexShouldAccessCookies()
    {
      var mockController = new Mock<TestPage> { CallBase = false };
      var mockHttpContext = new Mock<HttpContextBase>(MockBehavior.Strict);
      var mockHttpRequest = new Mock<HttpRequestBase>(MockBehavior.Strict);
      mockHttpRequest.SetupGet(req => req.Cookies).Returns(new HttpCookieCollection());
      mockHttpContext.SetupGet(ctx => ctx.Request).Returns(mockHttpRequest.Object);
      var requestContext = new RequestContext(mockHttpContext.Object, new RouteData());

      mockController.Object.ControllerContext = new ControllerContext(requestContext, mockController.Object);
      mockController.Object.Index();
      mockHttpRequest.VerifyGet(req => req.Cookies);
    }
  }
}
