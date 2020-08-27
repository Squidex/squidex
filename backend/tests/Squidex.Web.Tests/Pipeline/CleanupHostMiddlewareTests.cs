// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

        [Theory]
        [InlineData("https://cloud.squidex.io", "cloud.squidex.io")]
        [InlineData("https://cloud.squidex.io:5000", "cloud.squidex.io:5000")]
        [InlineData("http://cloud.squidex.io", "cloud.squidex.io")]
        [InlineData("http://cloud.squidex.io:5000", "cloud.squidex.io:5000")]
        public async Task Should_override_host_from_urls_options(string baseUrl, string expectedHost)
        {
            var uri = new Uri(baseUrl);

            var options = Options.Create(new UrlsOptions { BaseUrl = baseUrl });

            var sut = new CleanupHostMiddleware(next, options);

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Scheme = uri.Scheme;
            httpContext.Request.Host = new HostString(uri.Host, uri.Port);

            await sut.InvokeAsync(httpContext);

            Assert.Equal(expectedHost, httpContext.Request.Host.Value);
            Assert.True(isNextCalled);
        }
    }
}