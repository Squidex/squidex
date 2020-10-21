// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

#pragma warning disable RECS0092 // Convert field to readonly

namespace Squidex.Web.Pipeline
{
    public class CleanupHostMiddlewareTests
    {
        private readonly CleanupHostMiddleware sut;
        private bool isNextCalled;

        public CleanupHostMiddlewareTests()
        {
            Task Next(HttpContext context)
            {
                isNextCalled = true;

                return Task.CompletedTask;
            }

            sut = new CleanupHostMiddleware(Next);
        }

        [Fact]
        public async Task Should_cleanup_host_if_https_schema_contains_default_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("host", 443);

            await sut.InvokeAsync(httpContext);

            Assert.Null(httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_cleanup_host_if_http_schema_contains_default_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("host", 80);

            await sut.InvokeAsync(httpContext);

            Assert.Null(httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }

        [Fact]
        public async Task Should_not_cleanup_host_if_http_schema_contains_other_port()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("host", 8080);

            await sut.InvokeAsync(httpContext);

            Assert.Equal(8080, httpContext.Request.Host.Port);
            Assert.True(isNextCalled);
        }
    }
}