// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using AssignAppContributor = Squidex.Domain.Apps.Entities.Apps.Commands.AssignContributor;
using AssignTeamContributor = Squidex.Domain.Apps.Entities.Teams.Commands.AssignContributor;

namespace Squidex.Domain.Apps.Entities.Invitation;

public sealed class InviteUserCommandMiddleware : ICommandMiddleware
{
    private readonly IUserResolver userResolver;

    public InviteUserCommandMiddleware(IUserResolver userResolver)
    {
        this.userResolver = userResolver;
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is AssignAppContributor assignAppContributor)
        {
            var (userId, created) =
                await ResolveUserAsync(
                    assignAppContributor.ContributorId,
                    assignAppContributor.Invite,
                    ct);

            assignAppContributor.ContributorId = userId;

            await next(context, ct);

            if (created && context.PlainResult is IAppEntity app)
            {
                context.Complete(new InvitedResult<IAppEntity> { Entity = app });
            }
        }
        else if (context.Command is AssignTeamContributor assignTeamContributor)
        {
            var (userId, created) =
                await ResolveUserAsync(
                    assignTeamContributor.ContributorId,
                    assignTeamContributor.Invite,
                    ct);

            assignTeamContributor.ContributorId = userId;

            await next(context, ct);

            if (created && context.PlainResult is ITeamEntity team)
            {
                context.Complete(new InvitedResult<ITeamEntity> { Entity = team });
            }
        }
        else
        {
            await next(context, ct);
        }
    }

    private async Task<(string Id, bool)> ResolveUserAsync(string id, bool invite,
        CancellationToken ct)
    {
        if (!id.IsEmail())
        {
            return (id, false);
        }

        if (invite)
        {
            var (createdUser, created) = await userResolver.CreateUserIfNotExistsAsync(id, true, ct);

            return (createdUser?.Id ?? id, created);
        }

        var user = await userResolver.FindByIdOrEmailAsync(id, ct);

        return (user?.Id ?? id, false);
    }
}
