// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Squidex.Web.Pipeline
{
    public class ETagFilterTests
    {
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ActionExecutingContext executingContext;
        private readonly ActionExecutedContext executedContext;
        private readonly ETagFilter sut = new ETagFilter(Options.Create(new ETagOptions()));

        public ETagFilterTests()
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var filters = new List<IFilterMetadata>();

            executingContext = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object>(), this);
            executedContext = new ActionExecutedContext(actionContext, filters, this)
            {
                Result = new OkResult()
            };
        }

        [Fact]
        public async Task Should_not_convert_already_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = "W/13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_convert_strong_to_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal("W/13", httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_not_convert_empty_string_to_weak_tag()
        {
            httpContext.Response.Headers[HeaderNames.ETag] = string.Empty;

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(string.Empty, httpContext.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_return_304_for_same_etags()
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/13";

            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(304, ((StatusCodeResult)executedContext.Result).StatusCode);
        }

        [Fact]
        public async Task Should_not_return_304_for_different_etags()
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/11";

            httpContext.Response.Headers[HeaderNames.ETag] = "13";

            await sut.OnActionExecutionAsync(executingContext, Next());

            Assert.Equal(200, ((StatusCodeResult)executedContext.Result).StatusCode);
        }

        private ActionExecutionDelegate Next()
        {
            return () => Task.FromResult(executedContext);
        }
    }
}
