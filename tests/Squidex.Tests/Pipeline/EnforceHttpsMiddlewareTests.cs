// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Config;
using Squidex.Infrastructure.Tasks;
using Xunit;
using Options = Microsoft.Extensions.Options.Options;

namespace Squidex.Pipeline
{
    public class EnforceHttpsMiddlewareTests
    {
        private readonly RequestDelegate next;
        private bool isNextCalled;

        public EnforceHttpsMiddlewareTests()
        {
            next = (context) =>
            {
                isNextCalled = true;

                return TaskHelper.Done;
            };
        }

        [Fact]
        public async Task Should_make_permanent_redirect_if_redirect_is_required()
        {
            var httpContext = CreateHttpContext();

            var sut = new EnforceHttpsMiddleware(next, Options.Create(new MyUrlsOptions { EnforceHTTPS = true }));

            await sut.Invoke(httpContext);

            Assert.False(isNextCalled);
            Assert.Equal("https://squidex.local/path?query=1", httpContext.Response.Headers["Location"]);
        }

        [Fact]
        public async Task Should_not_redirect_if_already_on_https()
        {
            var httpContext = CreateHttpContext("https");

            var sut = new EnforceHttpsMiddleware(next, Options.Create(new MyUrlsOptions { EnforceHTTPS = true }));

            await sut.Invoke(httpContext);

            Assert.True(isNextCalled);
            Assert.Null((string)httpContext.Response.Headers["Location"]);
        }

        [Fact]
        public async Task Should_not_redirect_if_not_required()
        {
            var httpContext = CreateHttpContext("http");

            var sut = new EnforceHttpsMiddleware(next, Options.Create(new MyUrlsOptions { EnforceHTTPS = false }));

            await sut.Invoke(httpContext);

            Assert.True(isNextCalled);
            Assert.Null((string)httpContext.Response.Headers["Location"]);
        }

        private static DefaultHttpContext CreateHttpContext(string scheme = "http")
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.QueryString = new QueryString("?query=1");
            httpContext.Request.Host = new HostString("squidex.local");
            httpContext.Request.Path = new PathString("/path");
            httpContext.Request.Scheme = scheme;

            return httpContext;
        }
    }
}
