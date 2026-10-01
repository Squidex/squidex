// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities;

namespace Squidex.Web.Pipeline;

public class ContextFilterTests
{
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionExecutingContext executingContext;
    private readonly ContextFilter sut = new ContextFilter();

    public ContextFilterTests()
    {
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        executingContext = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), this);
    }

    [Fact]
    public async Task Should_copy_custom_headers()
    {
        httpContext.Request.Headers["X-Fields"] = "a,b";
        httpContext.Request.Headers["Accept"] = "application/json";

        var context = await ExecuteAsync();

        Assert.Equal("a,b", context.Headers["X-Fields"]);
        Assert.False(context.Headers.ContainsKey("Accept"));
    }

    [Theory]
    [InlineData("no-cache")]
    [InlineData("no-store")]
    [InlineData("max-age=0, no-cache")]
    public async Task Should_disable_query_cache_if_client_does_not_want_cached_responses(string cacheControl)
    {
        httpContext.Request.Headers[HeaderNames.CacheControl] = cacheControl;

        var context = await ExecuteAsync();

        Assert.True(context.NoQueryCache());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("max-age=60")]
    [InlineData("public")]
    public async Task Should_not_disable_query_cache_for_other_cache_control_values(string? cacheControl)
    {
        if (cacheControl != null)
        {
            httpContext.Request.Headers[HeaderNames.CacheControl] = cacheControl;
        }

        var context = await ExecuteAsync();

        Assert.False(context.NoQueryCache());
    }

    private async Task<Context> ExecuteAsync()
    {
        await sut.OnActionExecutionAsync(executingContext, () => Task.FromResult<ActionExecutedContext>(null!));

        return httpContext.Features.Get<Context>()!;
    }
}
