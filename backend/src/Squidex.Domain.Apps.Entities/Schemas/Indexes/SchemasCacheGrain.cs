// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Concurrency;
using Orleans.Core;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans.Indexes;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    [Reentrant]
    public sealed class SchemasCacheGrain : UniqueNameGrain<DomainId>, ISchemasCacheGrain
    {
        private readonly ISchemaRepository schemaRepository;
        private readonly Dictionary<string, DomainId> schemaIds = new Dictionary<string, DomainId>();
        private bool isLoaded;

        public SchemasCacheGrain(IGrainIdentity identity, ISchemaRepository schemaRepository)
            : base(identity)
        {
            this.schemaRepository = schemaRepository;
        }

        public override Task OnActivateAsync()
        {
            return GetIdsAsync();
        }

        public override async Task<string?> ReserveAsync(DomainId id, string name)
        {
            var token = await base.ReserveAsync(id, name);

            if (token == null)
            {
                return null;
            }

            var ids = await GetIdsAsync();

            if (ids.ContainsKey(name))
            {
                await RemoveReservationAsync(token);
                return null;
            }

            return token;
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
            if (!isLoaded)
            {
                var loaded = await schemaRepository.QueryIdsAsync(Key);

                foreach (var (name, id) in loaded)
                {
                    schemaIds[name] = id;
                }

                isLoaded = true;
            }

            return schemaIds;
        }

        public Task AddAsync(DomainId id, string name)
        {
            schemaIds[name] = id;

            return Task.CompletedTask;
        }

        public async Task RemoveAsync(DomainId id)
        {
            await GetIdsAsync();

            var name = schemaIds.FirstOrDefault(x => x.Value == id).Key;

            if (name != null)
            {
                schemaIds.Remove(name);
            }
        }
    }
}
