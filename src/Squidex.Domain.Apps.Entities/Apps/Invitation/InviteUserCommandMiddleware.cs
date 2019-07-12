// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public sealed class InviteUserCommandMiddleware : ICommandMiddleware
    {
        private readonly IUserResolver userResolver;

        public InviteUserCommandMiddleware(IUserResolver userResolver)
        {
            Guard.NotNull(userResolver, nameof(userResolver));

            this.userResolver = userResolver;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is AssignContributor assignContributor)
            {
                if (assignContributor.IsInviting && assignContributor.ContributorId.IsEmail())
                {
                    assignContributor.IsCreated = await userResolver.CreateUserIfNotExists(assignContributor.ContributorId, true);

                    await next();

                    if (assignContributor.IsCreated && context.PlainResult is IAppEntity app)
                    {
                        context.Complete(new InvitedResult { App = app });
                    }

                    return;
                }
            }

            await next();
        }
    }
}
