// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasByAppIndexCommandMiddleware : ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public SchemasByAppIndexCommandMiddleware(IGrainFactory grainFactory)
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
                    case CreateSchema createSchema:
                        await Index(createSchema.AppId.Id).AddSchemaAsync(createSchema.SchemaId, createSchema.Name);
                        break;
                    case DeleteSchema deleteSchema:
                        {
                            var schema = await grainFactory.GetGrain<ISchemaGrain>(deleteSchema.SchemaId).GetStateAsync();

                            await Index(schema.Value.AppId.Id).RemoveSchemaAsync(deleteSchema.SchemaId);

                            break;
                        }
                }
            }

            await next();
        }

        private ISchemasByAppIndex Index(Guid appId)
        {
            return grainFactory.GetGrain<ISchemasByAppIndex>(appId);
        }
    }
}
