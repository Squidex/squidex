// ==========================================================================
//  AppUserGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    [StorageProvider(ProviderName = "Default")]
    public sealed class AppUserGrain : Grain<HashSet<Guid>>, IAppUserGrain
    {
        public Task AddSchemaAsync(Guid schemaId)
        {
            State.Add(schemaId);

            return TaskHelper.Done;
        }

        public Task RemoveSchemaAsync(Guid schemaId)
        {
            State.Remove(schemaId);

            return TaskHelper.Done;
        }

        public Task<List<Guid>> GetSchemaIdsAsync()
        {
            return Task.FromResult(State.ToList());
        }
    }
}
