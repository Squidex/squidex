// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

#pragma warning disable RECS0092 // Convert field to readonly

namespace Squidex.Web.Pipeline
{
    public class CleanupHostMiddlewareTests
    {
        private readonly RequestDelegate next;
        private bool isNextCalled;

        public CleanupHostMiddlewareTests()
        {
            Task Next(HttpContext context)
            {
                isNextCalled = true;

                return Task.CompletedTask;
            }

            next = Next;
        }

        [Fact]
        public async Task Should_override_host_from_urls_options()
        {
            var options = Options.Create(new UrlsOptions { BaseUrl = "https://cloud.squidex.io" });

            var sut = new CleanupHostMiddleware(next, options);

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("host", 443);

            await sut.InvokeAsync(httpContext);

            Assert.Equal("cloud.squidex.io", httpContext.Request.Host.Value);
            Assert.True(isNextCalled);
        }
    }
}
