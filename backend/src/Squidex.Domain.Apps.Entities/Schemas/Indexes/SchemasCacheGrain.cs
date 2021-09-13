// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    [Reentrant]
    public sealed class SchemasCacheGrain : UniqueNameGrain<DomainId>, ISchemasCacheGrain
    {
        private readonly ISchemaRepository schemaRepository;
        private Dictionary<string, DomainId>? schemaIds;

        private DomainId AppId => DomainId.Create(Key);

        public SchemasCacheGrain(ISchemaRepository schemaRepository)
        {
            this.schemaRepository = schemaRepository;
        }

        public async Task<IReadOnlyCollection<DomainId>> GetSchemaIdsAsync()
        {
            var ids = await GetIdsAsync();

            return ids.Values;
        }

        public async Task<DomainId> GetSchemaIdAsync(string name)
        {
            var ids = await GetIdsAsync();

            return ids.GetOrDefault(name);
        }

        private async Task<Dictionary<string, DomainId>> GetIdsAsync()
        {
            var ids = schemaIds;

            if (ids == null)
            {
                ids = await schemaRepository.QueryIdsAsync(AppId);

                schemaIds = ids;
            }

            return ids;
        }

        public Task AddAsync(DomainId id, string name)
        {
            if (schemaIds != null)
            {
                schemaIds[name] = id;
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(DomainId id)
        {
            if (schemaIds != null)
            {
                var name = schemaIds.FirstOrDefault(x => x.Value == id).Key;

                if (name != null)
                {
                    schemaIds.Remove(name);
                }
            }

            return Task.CompletedTask;
        }
    }
}
