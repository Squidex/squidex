// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Config;
using Squidex.Pipeline;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class EnforceHttpsMiddlewareTests
    {
        private readonly Mock<RequestDelegate> next = new Mock<RequestDelegate>();
        private readonly Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
        private readonly Mock<HttpRequest> requestMock = new Mock<HttpRequest>();
        private readonly Mock<HttpResponse> responseMock = new Mock<HttpResponse>();
        private IOptions<MyUrlsOptions> urls;
        private EnforceHttpsMiddleware sut;

        public EnforceHttpsMiddlewareTests()
        {
            requestMock.Setup(x => x.Host).Returns(new HostString("test.squidex.com"));
            requestMock.Setup(x => x.Scheme).Returns("https");
            httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);
            httpContextMock.Setup(x => x.Response).Returns(responseMock.Object);
        }

        [Fact]
        public async Task Should_Continue_EnforceHTTPS_Is_False_Then_Return()
        {
            urls = new OptionsManager<MyUrlsOptions>(new OptionsFactory<MyUrlsOptions>(
                new List<IConfigureOptions<MyUrlsOptions>>(), new List<IPostConfigureOptions<MyUrlsOptions>>()));
            urls.Value.EnforceHTTPS = false;

            sut = new EnforceHttpsMiddleware(next.Object, urls);
            await sut.Invoke(httpContextMock.Object);

            next.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Should_Call_Next_If_EnforceHTTPS_Is_True_And_Request_Scheme_Is_Https()
        {
            urls = new OptionsManager<MyUrlsOptions>(new OptionsFactory<MyUrlsOptions>(
                new List<IConfigureOptions<MyUrlsOptions>>(), new List<IPostConfigureOptions<MyUrlsOptions>>()));
            urls.Value.EnforceHTTPS = true;

            sut = new EnforceHttpsMiddleware(next.Object, urls);
            await sut.Invoke(httpContextMock.Object);

            next.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Should_Call_Next_If_EnforceHTTPS_Is_False_Then_Return()
        {
            urls = new OptionsManager<MyUrlsOptions>(new OptionsFactory<MyUrlsOptions>(
                new List<IConfigureOptions<MyUrlsOptions>>(), new List<IPostConfigureOptions<MyUrlsOptions>>()));
            urls.Value.EnforceHTTPS = true;

            requestMock.Setup(x => x.Scheme).Returns("http");
            sut = new EnforceHttpsMiddleware(next.Object, urls);
            await sut.Invoke(httpContextMock.Object);

            next.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
            responseMock.Verify(x => x.Redirect(It.IsAny<string>(), true), Times.Once);
        }
    }
}
