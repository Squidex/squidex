﻿// ==========================================================================
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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Squidex.Web.Pipeline;

public class CachingFilterTests
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionExecutingContext executingContext;
    private readonly ActionExecutedContext executedContext;
    private readonly CachingOptions cachingOptions = new CachingOptions();
    private readonly CachingManager cachingManager;
    private readonly CachingFilter sut;

    public CachingFilterTests()
    {
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        cachingManager = new CachingManager(httpContextAccessor, Options.Create(cachingOptions));

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionFilters = new List<IFilterMetadata>();

        executingContext = new ActionExecutingContext(actionContext, actionFilters, new Dictionary<string, object?>(), this);
        executedContext = new ActionExecutedContext(actionContext, actionFilters, this)
        {
            Result = new OkResult()
        };

        sut = new CachingFilter(cachingManager);
    }

    [Theory]
    [InlineData("13", "13")]
    [InlineData("13", "W/13")]
    [InlineData("W/13", "13")]
    [InlineData("W/13", "W/13")]
    public async Task Should_return_304_for_same_etags(string ifNoneMatch, string etag)
    {
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Headers[HeaderNames.IfNoneMatch] = ifNoneMatch;

        httpContext.Response.Headers[HeaderNames.ETag] = etag;

        await sut.OnActionExecutionAsync(executingContext, Next());

        Assert.Equal(304, ((StatusCodeResult)executedContext.Result!).StatusCode);
    }

    [Fact]
    public async Task Should_return_304_for_same_etags_from_cache_manager()
    {
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "2C70E12B7A0646F92279F427C7B38E7334D8E5389CFF167A1DC30E73F826B683";

        await sut.OnActionExecutionAsync(executingContext, () =>
        {
            cachingManager.AddDependency("key");

            return Task.FromResult(executedContext);
        });

        Assert.Equal(304, ((StatusCodeResult)executedContext.Result!).StatusCode);
    }

    [Fact]
    public async Task Should_not_return_304_for_different_etags()
    {
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/13";

        httpContext.Response.Headers[HeaderNames.ETag] = "W/11";

        await sut.OnActionExecutionAsync(executingContext, Next());

        Assert.Equal(200, ((StatusCodeResult)executedContext.Result!).StatusCode);
    }

    [Fact]
    public async Task Should_not_return_304_for_post()
    {
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Headers[HeaderNames.IfNoneMatch] = "W/13";

        httpContext.Response.Headers[HeaderNames.ETag] = "W/13";

        await sut.OnActionExecutionAsync(executingContext, Next());

        Assert.Equal(200, ((StatusCodeResult)executedContext.Result!).StatusCode);
    }

    private ActionExecutionDelegate Next()
    {
        return () => Task.FromResult(executedContext);
    }
}
