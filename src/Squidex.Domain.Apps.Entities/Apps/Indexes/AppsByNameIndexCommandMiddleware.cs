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
            var createApp = context.Command as CreateApp;

            var isReserved = false;
            try
            {
                if (createApp != null)
                {
                    isReserved = await index.ReserveAppAsync(createApp.AppId, createApp.Name);

                    if (!isReserved)
                    {
                        var error = new ValidationError("An app with the same name already exists.", nameof(createApp.Name));

                        throw new ValidationException("Cannot create app.", error);
                    }
                }

                await next();

                if (context.IsCompleted)
                {
                    if (createApp != null)
                    {
                        await index.AddAppAsync(createApp.AppId, createApp.Name);
                    }
                    else if (context.Command is ArchiveApp archiveApp)
                    {
                        await index.RemoveAppAsync(archiveApp.AppId);
                    }
                }
            }
            finally
            {
                if (isReserved)
                {
                    await index.RemoveReservationAsync(createApp.AppId, createApp.Name);
                }
            }
        }
    }
}
