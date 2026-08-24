// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Core.Apps;
using RequestContext = Squidex.Domain.Apps.Entities.Context;

namespace Squidex.Web;

public class ContextProviderTests
{
    private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly App app = new App { Name = "my-app" };
    private readonly ContextProvider sut;

    public ContextProviderTests()
    {
        // A fake would return a dummy http context, but the fallback is only used when it is null.
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(null);

        sut = new ContextProvider(httpContextAccessor);
    }

    private void UseHttpContext()
    {
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(httpContext);
    }

    [Fact]
    public void Should_provide_context_from_http_context()
    {
        UseHttpContext();

        var context = new RequestContext(httpContext.User, app);

        httpContext.Features.Set(context);

        Assert.Same(context, sut.Context);
    }

    [Fact]
    public void Should_create_context_when_http_context_has_none()
    {
        UseHttpContext();

        Assert.NotNull(sut.Context);
    }

    [Fact]
    public void Should_reuse_created_context_of_http_context()
    {
        UseHttpContext();

        Assert.Same(sut.Context, sut.Context);
    }

    [Fact]
    public void Should_read_headers_from_request()
    {
        UseHttpContext();

        httpContext.Request.Headers["X-Fields"] = "a,b";

        Assert.Equal("a,b", sut.Context.Headers["X-Fields"]);
    }

    [Fact]
    public void Should_write_context_to_http_context()
    {
        UseHttpContext();

        var context = new RequestContext(httpContext.User, app);

        sut.Context = context;

        Assert.Same(context, httpContext.Features.Get<RequestContext>());
        Assert.Same(context, sut.Context);
    }

    [Fact]
    public void Should_replace_context_of_http_context()
    {
        UseHttpContext();

        var context1 = sut.Context;
        var context2 = context1.Clone(b => b.SetHeader("X-Fields", "a,b"));

        sut.Context = context2;

        Assert.NotSame(context1, sut.Context);
        Assert.Same(context2, sut.Context);
    }

    [Fact]
    public void Should_provide_anonymous_context_without_http_context()
    {
        Assert.NotNull(sut.Context);
    }

    [Fact]
    public void Should_reuse_anonymous_context_without_http_context()
    {
        Assert.Same(sut.Context, sut.Context);
    }

    [Fact]
    public void Should_write_context_without_http_context()
    {
        var context = new RequestContext(httpContext.User, app);

        sut.Context = context;

        Assert.Same(context, sut.Context);
    }
}
