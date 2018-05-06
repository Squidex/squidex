// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasByAppIndexGrain : GrainOfGuid, ISchemasByAppIndex
    {
        private readonly IStore<Guid> store;
        private IPersistence<State> persistence;
        private State state = new State();

        [CollectionName("Index_SchemasByApp")]
        public sealed class State
        {
            public Dictionary<string, Guid> Schemas { get; set; } = new Dictionary<string, Guid>();
        }

        public SchemasByAppIndexGrain(IStore<Guid> store)
        {
            Guard.NotNull(store, nameof(store));

            this.store = store;
        }

        public override Task OnActivateAsync(Guid key)
        {
            persistence = store.WithSnapshots<SchemasByAppIndexGrain, State, Guid>(key, s =>
            {
                state = s;
            });

            return persistence.ReadAsync();
        }

        public Task RebuildAsync(Dictionary<string, Guid> schemas)
        {
            state = new State { Schemas = schemas };

            return persistence.WriteSnapshotAsync(state);
        }

        public Task AddSchemaAsync(Guid schemaId, string name)
        {
            state.Schemas[name] = schemaId;

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveSchemaAsync(Guid schemaId)
        {
            state.Schemas.Remove(state.Schemas.FirstOrDefault(x => x.Value == schemaId).Key ?? string.Empty);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task<Guid> GetSchemaIdAsync(string name)
        {
            state.Schemas.TryGetValue(name, out var schemaId);

            return Task.FromResult(schemaId);
        }

        public Task<List<Guid>> GetSchemaIdsAsync()
        {
            return Task.FromResult(state.Schemas.Values.ToList());
        }
    }
}
