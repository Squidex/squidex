// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web.CommandMiddlewares;

public sealed class EnrichWithTeamIdCommandMiddleware : ICommandMiddleware
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public EnrichWithTeamIdCommandMiddleware(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            return next(context, ct);
        }

        if (context.Command is ITeamCommand teamCommand && teamCommand.TeamId == default)
        {
            var teamId = GetTeamId();

            teamCommand.TeamId = teamId;
        }

        return next(context, ct);
    }

    private DomainId GetTeamId()
    {
        var feature = httpContextAccessor.HttpContext?.Features.Get<ITeamFeature>();

        if (feature == null)
        {
            ThrowHelper.InvalidOperationException("Cannot resolve team.");
            return default!;
        }

        return feature.Team.Id;
    }
}
