// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline;

public class AppResolverTests : GivenContext
{
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionContext actionContext;
    private readonly ActionExecutingContext actionExecutingContext;
    private readonly ActionExecutionDelegate next;
    private readonly AppResolver sut;
    private bool isNextCalled;

    public AppResolverTests()
    {
        actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
        });

        actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), this);
        actionExecutingContext.HttpContext = httpContext;
        actionExecutingContext.RouteData.Values["app"] = AppId.Name;

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new AppResolver(AppProvider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_return_404_if_app_name_is_null(string? app)
    {
        SetupUser();

        actionExecutingContext.RouteData.Values["app"] = app;

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);

        A.CallTo(() => AppProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_404_if_app_not_found()
    {
        SetupUser();

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, false, httpContext.RequestAborted))
            .Returns(Task.FromResult<App?>(null));

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_401_if_user_is_anonymous()
    {
        SetupUser(null);

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, false, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_user()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, $"squidex.apps.{AppId.Name}"));

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, true, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(App, httpContext.Context().App);
        Assert.True(user.Claims.Any());
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.TrueForAll(x => x.Value.StartsWith($"squidex.apps.{AppId.Name}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_contributor()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));

        App = App with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Reader)
        };

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, true, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(App, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.TrueForAll(x => x.Value.StartsWith($"squidex.apps.{AppId.Name}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_provide_extra_permissions_if_client_is_frontend()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));
        user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

        App = App with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Reader)
        };

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, false, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(App, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count > 10);
        Assert.True(permissions.TrueForAll(x => x.Value.StartsWith($"squidex.apps.{AppId.Name}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_client()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{AppId.Name}:{Client.Identifier}"));

        App = App with
        {
            Clients = AppClients.Empty.Add(Client.Identifier, Role.Reader)
        };

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, true, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(App, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_anonymous_client()
    {
        var user = SetupUser();

        App = App with
        {
            Clients = AppClients.Empty.Add(Client.Identifier, Role.Reader).Update(Client.Identifier, allowAnonymous: true)
        };

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, true, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(App, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(isNextCalled);
        Assert.Contains(user.Claims, x => x.Type == OpenIdClaims.ClientId && x.Value == Client.Identifier);
    }

    [Fact]
    public async Task Should_resolve_app_if_action_allows_anonymous_but_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{AppId.Name}:{Client.Identifier}"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        actionContext.ActionDescriptor.EndpointMetadata.Add(new AllowAnonymousAttribute());

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, true, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(App, httpContext.Context().App);
        Assert.Equal(2, user.Claims.Count());
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{AppId.Name}:{Client.Identifier}"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, false, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_client_is_from_another_app()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"other:{Client.Identifier}"));

        App = App with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Reader)
        };

        A.CallTo(() => AppProvider.GetAppAsync(AppId.Name, false, httpContext.RequestAborted))
            .Returns(App);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_do_nothing_if_parameter_not_set()
    {
        actionExecutingContext.RouteData.Values.Remove("app");

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => AppProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    private ClaimsIdentity SetupUser(string? type = "OIDC")
    {
        var userIdentity = new ClaimsIdentity(type);
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        actionExecutingContext.HttpContext.User = userPrincipal;

        return userIdentity;
    }
}
