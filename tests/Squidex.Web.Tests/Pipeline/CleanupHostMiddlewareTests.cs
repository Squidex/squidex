// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class CleanupHostMiddlewareTests
    {
        private readonly RequestDelegate next;
        private readonly CleanupHostMiddleware sut;
        private bool isNextCalled;

        public CleanupHostMiddlewareTests()
        {
            next = context =>
            {
                isNextCalled = true;

                return TaskHelper.Done;
            };

            sut = new CleanupHostMiddleware(next);
        }

        [Fact]
        public async Task Should_cleanup_host_if_https_schema_contains_default_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("host", 443);

            await sut.Invoke(httpContext);

            Assert.Null(httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_cleanup_host_if_http_schema_contains_default_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("host", 80);

            await sut.Invoke(httpContext);

            Assert.Null(httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_not_cleanup_host_if_http_schema_contains_other_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("host", 8080);

            await sut.Invoke(httpContext);

            Assert.Equal(8080, httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }
    }
}
