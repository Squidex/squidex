// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByUserIndexCommandMiddleware : ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public AppsByUserIndexCommandMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case CreateApp createApp:
                        await Index(GetUserId(createApp)).AddAppAsync(createApp.AppId);
                        break;
                    case AssignContributor assignContributor:
                        await Index(GetUserId(context)).AddAppAsync(assignContributor.AppId);
                        break;
                    case RemoveContributor removeContributor:
                        await Index(GetUserId(removeContributor)).RemoveAppAsync(removeContributor.AppId);
                        break;
                    case ArchiveApp archiveApp:
                        {
                            var appState = await grainFactory.GetGrain<IAppGrain>(archiveApp.AppId).GetStateAsync();

                            foreach (var contributorId in appState.Value.Contributors.Keys)
                            {
                                await Index(contributorId).RemoveAppAsync(archiveApp.AppId);
                            }

                            break;
                        }
                }
            }

            await next();
        }

        private static string GetUserId(RemoveContributor removeContributor)
        {
            return removeContributor.ContributorId;
        }

        private static string GetUserId(CreateApp createApp)
        {
            return createApp.Actor.Identifier;
        }

        private static string GetUserId(CommandContext context)
        {
            return context.Result<EntityCreatedResult<string>>().IdOrValue;
        }

        private IAppsByUserIndex Index(string id)
        {
            return grainFactory.GetGrain<IAppsByUserIndex>(id);
        }
    }
}
