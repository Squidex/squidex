// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class FileCallbackResultTests
    {
        private readonly ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
        private readonly ActionContext context;
        private readonly Mock<ActionDescriptor> actionDescriptor = new Mock<ActionDescriptor>();
        private readonly Mock<HttpContext> httpContext = new Mock<HttpContext>();
        private readonly Mock<HttpRequest> requestMock = new Mock<HttpRequest>();
        private readonly Mock<HttpResponse> responseMock = new Mock<HttpResponse>();
        private readonly Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
        private readonly Func<Stream, Task> callback;
        private readonly FileCallbackResult sut;
        private readonly FileCallbackResultExecutor callbackExecutor;
        private bool callbackWasCalled;

        public FileCallbackResultTests()
        {
            requestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
            responseMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
            httpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);
            httpContext.Setup(x => x.Request).Returns(requestMock.Object);
            httpContext.Setup(x => x.Response).Returns(responseMock.Object);

            context = new ActionContext(httpContext.Object, new RouteData(), actionDescriptor.Object);
            callback = async bodyStream => { callbackWasCalled = true; };
            callbackExecutor = new FileCallbackResultExecutor(loggerFactory);
            sut = new FileCallbackResult("text/plain", "test.txt", callback);
        }

        [Fact]
        public async Task Should_Execute_Callback_Function()
        {
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(callbackExecutor);
            await sut.ExecuteResultAsync(context);

            Assert.True(callbackWasCalled);
        }

        [Fact]
        public async Task Should_Not_Call_Callback_If_Exception_Thrown_While_Logging()
        {
            httpContext.Setup(x => x.Request).Returns((HttpRequest)null);
            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(callbackExecutor);
            await sut.ExecuteResultAsync(context);

            Assert.False(callbackWasCalled);
        }
    }
}
