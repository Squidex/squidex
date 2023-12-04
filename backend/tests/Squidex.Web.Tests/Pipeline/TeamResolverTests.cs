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
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline;

public class TeamResolverTests : GivenContext
{
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionContext actionContext;
    private readonly ActionExecutingContext actionExecutingContext;
    private readonly ActionExecutionDelegate next;
    private readonly TeamResolver sut;
    private bool isNextCalled;

    public TeamResolverTests()
    {
        actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            EndpointMetadata = new List<object>()
        });

        actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), this);
        actionExecutingContext.HttpContext = httpContext;
        actionExecutingContext.RouteData.Values["team"] = TeamId.ToString();

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new TeamResolver(AppProvider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_return_404_if_team_name_is_null(string? team)
    {
        SetupUser();

        actionExecutingContext.RouteData.Values["team"] = team;

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);

        A.CallTo(() => AppProvider.GetTeamAsync(A<DomainId>._, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_404_if_team_not_found()
    {
        SetupUser();

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Task.FromResult<Team?>(null));

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_401_if_user_is_anonymous()
    {
        SetupUser(null);

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_from_user()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, $"squidex.teams.{TeamId}"));

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(Team, httpContext.Features.Get<Team>());
        Assert.True(user.Claims.Any());
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.TrueForAll(x => x.Value.StartsWith($"squidex.teams.{TeamId}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_from_contributor()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.Subject, User.Identifier));

        Team = Team with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, Role.Reader)
        };

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(Team, httpContext.Features.Get<Team>());
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.TrueForAll(x => x.Value.StartsWith($"squidex.teams.{TeamId}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_if_action_allows_anonymous_but_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{TeamId}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.teams.other-team"));

        actionContext.ActionDescriptor.EndpointMetadata.Add(new AllowAnonymousAttribute());

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(Team, httpContext.Features.Get<Team>());
        Assert.Equal(2, user.Claims.Count());
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{TeamId}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.teams.other-team"));

        A.CallTo(() => AppProvider.GetTeamAsync(TeamId, httpContext.RequestAborted))
            .Returns(Team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_do_nothing_if_parameter_not_set()
    {
        actionExecutingContext.RouteData.Values.Remove("team");

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.True(isNextCalled);

        A.CallTo(() => AppProvider.GetTeamAsync(A<DomainId>._, httpContext.RequestAborted))
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
