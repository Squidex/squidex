// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Web.Scripting;

public class HttpRequestJintExtensionTests
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly JintScriptEngine sut;

    public HttpRequestJintExtensionTests()
    {
        var extensions = new IJintExtension[]
        {
            new HttpRequestJintExtension(httpContextAccessor)
        };

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }), extensions);
    }

    [Fact]
    public void Should_get_null_request()
    {
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(null);

        const string script = @"
                getRequest();
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("null", actual);
    }

    [Fact]
    public void Should_get_method()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "DELETE";

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().method;
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("DELETE", actual);
    }

    [Fact]
    public void Should_get_path()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/path/to/request";

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().path;
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("/path/to/request", actual);
    }

    [Fact]
    public void Should_get_query_string()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?key=value");

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().queryString;
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("?key=value", actual);
    }

    [Fact]
    public void Should_get_query_string_if_null()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = default;

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().queryString;
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void Should_get_query()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?a=1&a=2&b=3");

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().query;
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("{\"a\":[\"1\", \"2\"], \"b\":[\"3\"]}", actual);
    }

    [Fact]
    public void Should_get_concrete_query()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?a=1&a=2&b=3");

        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);

        const string script = @"
                getRequest().query.a[1];
            ";

        var actual = sut.Execute([], script).ToString();

        Assert.Equal("2", actual);
    }
}
