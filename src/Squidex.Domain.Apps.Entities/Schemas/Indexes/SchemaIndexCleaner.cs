// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemaIndexCleaner : IAppStorage
    {
        private readonly IGrainFactory grainFactory;

        public SchemaIndexCleaner(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task ClearAsync(Guid appId)
        {
            return grainFactory.GetGrain<ISchemasByAppIndex>(appId).ClearAsync();
        }
    }
}
