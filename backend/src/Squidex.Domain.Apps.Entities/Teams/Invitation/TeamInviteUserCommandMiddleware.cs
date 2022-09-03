// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;

namespace Squidex.Domain.Teams.Apps.Teams.Invitation
{
    public sealed class TeamInviteUserCommandMiddleware : ICommandMiddleware
    {
        private readonly IUserResolver userResolver;

        public TeamInviteUserCommandMiddleware(IUserResolver userResolver)
        {
            this.userResolver = userResolver;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next,
            CancellationToken ct)
        {
            if (context.Command is AssignContributor assignContributor && ShouldResolve(assignContributor))
            {
                IUser? user;

                var created = false;

                if (assignContributor.Invite)
                {
                    (user, created) = await userResolver.CreateUserIfNotExistsAsync(assignContributor.ContributorId, true, ct);
                }
                else
                {
                    user = await userResolver.FindByIdOrEmailAsync(assignContributor.ContributorId, ct);
                }

                if (user != null)
                {
                    assignContributor.ContributorId = user.Id;
                }

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

        private static bool ShouldResolve(AssignContributor assignContributor)
        {
            return assignContributor.ContributorId.IsEmail();
        }
    }
}
