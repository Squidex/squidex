// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Squidex.Infrastructure.Log;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class ActionContextLogAppenderTests
    {
        private readonly Mock<IActionContextAccessor> actionContextAccessor = new Mock<IActionContextAccessor>();
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
        private readonly RouteData routeData = new RouteData();
        private readonly Guid requestId = Guid.NewGuid();
        private readonly IDictionary<object, object> items = new Dictionary<object, object>();
        private readonly IObjectWriter writer = A.Fake<IObjectWriter>();
        private readonly HttpRequest request = A.Fake<HttpRequest>();
        private ActionContextLogAppender sut;
        private ActionContext actionContext;

        [Fact]
        public void Append_should_get_requestId()
        {
            items.Add(nameof(requestId), requestId);
            SetupTest();

            A.CallTo(() => writer.WriteObject(It.IsAny<string>(), It.IsAny<Action<IObjectWriter>>())).Returns(writer);
            sut.Append(writer);

            Assert.NotNull(writer);
        }

        [Fact]
        public void Append_should_put_requestId()
        {
            SetupTest();

            sut.Append(writer);
        }

        [Fact]
        public void Append_should_return_if_no_actionContext()
        {
            sut = new ActionContextLogAppender(actionContextAccessor.Object);

            sut.Append(writer);
        }

        [Fact]
        public void Append_should_return_if_no_httpContext_method()
        {
            A.CallTo(() => request.Method).Returns(string.Empty);
            httpContextMock.Setup(x => x.Request).Returns(request);
            actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor.Object);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            sut = new ActionContextLogAppender(actionContextAccessor.Object);

            sut.Append(writer);
        }

        private void SetupTest()
        {
            A.CallTo(() => request.Method).Returns("Get");
            httpContextMock.Setup(x => x.Items).Returns(items);
            httpContextMock.Setup(x => x.Request).Returns(request);
            actionContext = new ActionContext(httpContextMock.Object, routeData, actionDescriptor.Object);
            actionContextAccessor.Setup(x => x.ActionContext).Returns(actionContext);
            sut = new ActionContextLogAppender(actionContextAccessor.Object);
        }
    }
}
