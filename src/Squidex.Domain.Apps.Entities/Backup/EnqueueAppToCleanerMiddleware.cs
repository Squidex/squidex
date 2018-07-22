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

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class EnqueueAppToCleanerMiddleware : ICommandMiddleware
    {
        private readonly IAppCleanerGrain cleaner;

        public EnqueueAppToCleanerMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            cleaner = grainFactory.GetGrain<IAppCleanerGrain>(SingleGrain.Id);
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case ArchiveApp archiveApp:
                        await cleaner.EnqueueAppAsync(archiveApp.AppId);
                        break;
                }
            }

            await next();
        }
    }
}
