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
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline;

public class AppResolverTests
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionContext actionContext;
    private readonly ActionExecutingContext actionExecutingContext;
    private readonly ActionExecutionDelegate next;
    private readonly string appName = "my-app";
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
        actionExecutingContext.RouteData.Values["app"] = appName;

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new AppResolver(appProvider);
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

        A.CallTo(() => appProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_404_if_app_not_found()
    {
        SetupUser();

        A.CallTo(() => appProvider.GetAppAsync(appName, false, httpContext.RequestAborted))
            .Returns(Task.FromResult<IAppEntity?>(null));

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_401_if_user_is_anonymous()
    {
        SetupUser(null);

        var app = CreateApp(appName);

        A.CallTo(() => appProvider.GetAppAsync(appName, false, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_user()
    {
        var user = SetupUser();

        var app = CreateApp(appName);

        user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, $"squidex.apps.{appName}"));

        A.CallTo(() => appProvider.GetAppAsync(appName, true, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(app, httpContext.Context().App);
        Assert.True(user.Claims.Any());
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.All(x => x.Value.StartsWith($"squidex.apps.{appName}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_contributor()
    {
        var user = SetupUser();

        var app = CreateApp(appName, user: "user1");

        user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));

        A.CallTo(() => appProvider.GetAppAsync(appName, true, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(app, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.All(x => x.Value.StartsWith($"squidex.apps.{appName}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_provide_extra_permissions_if_client_is_frontend()
    {
        var user = SetupUser();

        var app = CreateApp(appName, user: "user1");

        user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));
        user.AddClaim(new Claim(OpenIdClaims.ClientId, DefaultClients.Frontend));

        A.CallTo(() => appProvider.GetAppAsync(appName, false, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(app, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count > 10);
        Assert.True(permissions.All(x => x.Value.StartsWith($"squidex.apps.{appName}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_client()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{appName}:client1"));

        var app = CreateApp(appName, client: "client1");

        A.CallTo(() => appProvider.GetAppAsync(appName, true, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(app, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_app_from_anonymous_client()
    {
        var user = SetupUser();

        var app = CreateApp(appName, client: "client1", allowAnonymous: true);

        A.CallTo(() => appProvider.GetAppAsync(appName, true, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(app, httpContext.Context().App);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(isNextCalled);
        Assert.Contains(user.Claims, x => x.Type == OpenIdClaims.ClientId && x.Value == "client1");
    }

    [Fact]
    public async Task Should_resolve_app_if_action_allows_anonymous_but_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{appName}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        var app = CreateApp(appName);

        actionContext.ActionDescriptor.EndpointMetadata.Add(new AllowAnonymousAttribute());

        A.CallTo(() => appProvider.GetAppAsync(appName, true, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(app, httpContext.Context().App);
        Assert.Equal(2, user.Claims.Count());
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{appName}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.apps.other-app"));

        var app = CreateApp(appName);

        A.CallTo(() => appProvider.GetAppAsync(appName, false, httpContext.RequestAborted))
            .Returns(app);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_client_is_from_another_app()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, "other:client1"));

        var app = CreateApp(appName, client: "client1");

        A.CallTo(() => appProvider.GetAppAsync(appName, false, httpContext.RequestAborted))
            .Returns(app);

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

        A.CallTo(() => appProvider.GetAppAsync(A<string>._, false, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    private ClaimsIdentity SetupUser(string? type = "OIDC")
    {
        var userIdentity = new ClaimsIdentity(type);
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        actionExecutingContext.HttpContext.User = userPrincipal;

        return userIdentity;
    }

    private static IAppEntity CreateApp(string name, string? user = null, string? client = null, bool? allowAnonymous = null)
    {
        var app = A.Fake<IAppEntity>();

        var contributors = Contributors.Empty;

        if (user != null)
        {
            contributors = contributors.Assign(user, Role.Reader);
        }

        var clients = AppClients.Empty;

        if (client != null)
        {
            clients = clients.Add(client, "secret").Update(client, allowAnonymous: allowAnonymous);
        }

        A.CallTo(() => app.Contributors).Returns(contributors);
        A.CallTo(() => app.Clients).Returns(clients);
        A.CallTo(() => app.Name).Returns(name);
        A.CallTo(() => app.Roles).Returns(Roles.Empty);

        return app;
    }
}
