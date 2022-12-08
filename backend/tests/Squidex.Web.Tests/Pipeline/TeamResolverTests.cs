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
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Web.Pipeline;

public class TeamResolverTests
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly HttpContext httpContext = new DefaultHttpContext();
    private readonly ActionContext actionContext;
    private readonly ActionExecutingContext actionExecutingContext;
    private readonly ActionExecutionDelegate next;
    private readonly DomainId teamId = DomainId.NewGuid();
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
        actionExecutingContext.RouteData.Values["team"] = teamId.ToString();

        next = () =>
        {
            isNextCalled = true;

            return Task.FromResult<ActionExecutedContext>(null!);
        };

        sut = new TeamResolver(appProvider);
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

        A.CallTo(() => appProvider.GetTeamAsync(A<DomainId>._, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_404_if_team_not_found()
    {
        SetupUser();

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(Task.FromResult<ITeamEntity?>(null));

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<NotFoundResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_return_401_if_user_is_anonymous()
    {
        SetupUser(null);

        var team = CreateTeam(teamId);

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        Assert.False(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_from_user()
    {
        var user = SetupUser();

        var team = CreateTeam(teamId);

        user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, $"squidex.teams.{teamId}"));

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(team, httpContext.Features.Get<ITeamFeature>()!.Team);
        Assert.True(user.Claims.Any());
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.All(x => x.Value.StartsWith($"squidex.teams.{teamId}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_from_contributor()
    {
        var user = SetupUser();

        var team = CreateTeam(teamId, user: "user1");

        user.AddClaim(new Claim(OpenIdClaims.Subject, "user1"));

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        var permissions = user.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).ToList();

        Assert.Same(team, httpContext.Features.Get<ITeamFeature>()!.Team);
        Assert.True(user.Claims.Count() > 2);
        Assert.True(permissions.Count < 3);
        Assert.True(permissions.All(x => x.Value.StartsWith($"squidex.teams.{teamId}", StringComparison.OrdinalIgnoreCase)));
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_resolve_team_if_action_allows_anonymous_but_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{teamId}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.teams.other-team"));

        var team = CreateTeam(teamId);

        actionContext.ActionDescriptor.EndpointMetadata.Add(new AllowAnonymousAttribute());

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(team);

        await sut.OnActionExecutionAsync(actionExecutingContext, next);

        Assert.Same(team, httpContext.Features.Get<ITeamFeature>()!.Team);
        Assert.Equal(2, user.Claims.Count());
        Assert.True(isNextCalled);
    }

    [Fact]
    public async Task Should_return_404_if_user_has_no_permissions()
    {
        var user = SetupUser();

        user.AddClaim(new Claim(OpenIdClaims.ClientId, $"{teamId}:client1"));
        user.AddClaim(new Claim(SquidexClaimTypes.Permissions, "squidex.teams.other-team"));

        var team = CreateTeam(teamId);

        A.CallTo(() => appProvider.GetTeamAsync(teamId, httpContext.RequestAborted))
            .Returns(team);

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

        A.CallTo(() => appProvider.GetTeamAsync(A<DomainId>._, httpContext.RequestAborted))
            .MustNotHaveHappened();
    }

    private ClaimsIdentity SetupUser(string? type = "OIDC")
    {
        var userIdentity = new ClaimsIdentity(type);
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        actionExecutingContext.HttpContext.User = userPrincipal;

        return userIdentity;
    }

    private static ITeamEntity CreateTeam(DomainId id, string? user = null)
    {
        var team = A.Fake<ITeamEntity>();

        var contributors = Contributors.Empty;

        if (user != null)
        {
            contributors = contributors.Assign(user, Role.Owner);
        }

        A.CallTo(() => team.Id).Returns(id);
        A.CallTo(() => team.Contributors).Returns(contributors);

        return team;
    }
}
