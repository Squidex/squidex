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
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Web.Pipeline;

public class ApiCostsFilterTests : GivenContext
{
    private readonly IUsageGate usageGate = A.Fake<IUsageGate>();
    private readonly ActionExecutingContext actionContext;
    private readonly ActionExecutionDelegate next;
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ApiCostsFilter sut;
    private bool isNextCalled;

    public ApiCostsFilterTests()
    {
        actionContext =
            new ActionExecutingContext(
                new ActionContext(httpContext, new RouteData(),
                    new ActionDescriptor()),
                new List<IFilterMetadata>(), new Dictionary<string, object?>(), null!);

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new ApiCostsFilter(usageGate);
    }

    [Fact]
    public async Task Should_return_429_status_code_if_blocked()
    {
        sut.FilterDefinition = new ApiCostsAttribute(1);

        SetupApp();

        A.CallTo(() => usageGate.IsBlockedAsync(App, A<string>._, DateTime.Today.ToDateOnly(), default))
            .Returns(true);

        await sut.OnActionExecutionAsync(actionContext, next);

        Assert.Equal(429, (actionContext.Result as StatusCodeResult)?.StatusCode);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_continue_if_not_blocked()
    {
        sut.FilterDefinition = new ApiCostsAttribute(13);

        SetupApp();

        A.CallTo(() => usageGate.IsBlockedAsync(App, A<string>._, DateTime.Today.ToDateOnly(), default))
            .Returns(false);

        await sut.OnActionExecutionAsync(actionContext, next);

        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_continue_if_costs_are_zero()
    {
        sut.FilterDefinition = new ApiCostsAttribute(0);

        SetupApp();

        await sut.OnActionExecutionAsync(actionContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => usageGate.IsBlockedAsync(App, A<string>._, A<DateOnly>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_continue_if_not_app_request()
    {
        sut.FilterDefinition = new ApiCostsAttribute(12);

        await sut.OnActionExecutionAsync(actionContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => usageGate.IsBlockedAsync(App, A<string>._, A<DateOnly>._, default))
            .MustNotHaveHappened();
    }

    private void SetupApp()
    {
        httpContext.Features.Set(Context.Anonymous(App));
    }
}
