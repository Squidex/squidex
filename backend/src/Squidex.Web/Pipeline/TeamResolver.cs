// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web.Pipeline;

public sealed class TeamResolver : IAsyncActionFilter
{
    private readonly IAppProvider appProvider;

    public TeamResolver(IAppProvider appProvider)
    {
        this.appProvider = appProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (context.RouteData.Values.TryGetValue("team", out var teamValue))
        {
            var teamId = teamValue?.ToString();

            if (string.IsNullOrWhiteSpace(teamId))
            {
                context.Result = new NotFoundResult();
                return;
            }

            var team = await appProvider.GetTeamAsync(DomainId.Create(teamId), default);

            if (team == null)
            {
                var log = context.HttpContext.RequestServices?.GetService<ILogger<TeamResolver>>();

                log?.LogWarning("Cannot find team with the given id {id}.", teamId);

                context.Result = new NotFoundResult();
                return;
            }

            var subjectId = user.OpenIdSubject();

            if (subjectId != null && team.Contributors.TryGetValue(subjectId, out var role))
            {
                var identity = user.Identities.First();

                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.ForApp(PermissionIds.TeamAdmin, team: team.Id.ToString()).Id));
            }

            var requestContext = new Context(context.HttpContext.User, null!).WithHeaders(context.HttpContext);

            if (!AllowAnonymous(context) && !HasPermission(team.Id, requestContext))
            {
                if (string.IsNullOrWhiteSpace(user.Identity?.AuthenticationType))
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    var log = context.HttpContext.RequestServices?.GetService<ILogger<AppResolver>>();

                    log?.LogWarning("Authenticated user has no permission to access the team with ID {id}.", team.Id);

                    context.Result = new NotFoundResult();
                }

                return;
            }

            context.HttpContext.Features.Set(requestContext);
            context.HttpContext.Features.Set<ITeamFeature>(new TeamFeature(team));
            context.HttpContext.Response.Headers.Add("X-TeamId", team.Id.ToString());
        }

        await next();
    }

    private static bool HasPermission(DomainId teamId, Context requestContext)
    {
        return requestContext.UserPermissions.Includes(PermissionIds.ForApp(PermissionIds.Team, team: teamId.ToString()));
    }

    private static bool AllowAnonymous(ActionExecutingContext context)
    {
        return context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute);
    }
}
