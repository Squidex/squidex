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
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public sealed class AppsByNameIndexCommandMiddleware : ICommandMiddleware
    {
        private readonly IAppsByNameIndex index;

        public AppsByNameIndexCommandMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case CreateApp createApp:
                        await index.AddAppAsync(createApp.AppId, createApp.Name);
                        break;
                    case ArchiveApp archiveApp:
                        await index.RemoveAppAsync(archiveApp.AppId);
                        break;
                }
            }

            await next();
        }
    }
}
