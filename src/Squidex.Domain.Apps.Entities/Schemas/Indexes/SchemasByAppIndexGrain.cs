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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasByAppIndexGrain : GrainOfGuid<SchemasByAppIndexGrain.GrainState>, ISchemasByAppIndex
    {
        [CollectionName("Index_SchemasByApp")]
        public sealed class GrainState
        {
            public Dictionary<string, Guid> Schemas { get; set; } = new Dictionary<string, Guid>();
        }

        public SchemasByAppIndexGrain(IStore<Guid> store)
            : base(store)
        {
        }

        public Task ClearAsync()
        {
            return ClearStateAsync();
        }

        public Task RebuildAsync(Dictionary<string, Guid> schemas)
        {
            State = new GrainState { Schemas = schemas };

            return WriteStateAsync();
        }

        public Task AddSchemaAsync(Guid schemaId, string name)
        {
            State.Schemas[name] = schemaId;

            return WriteStateAsync();
        }

        public Task RemoveSchemaAsync(Guid schemaId)
        {
            State.Schemas.Remove(State.Schemas.FirstOrDefault(x => x.Value == schemaId).Key ?? string.Empty);

            return WriteStateAsync();
        }

        public Task<Guid> GetSchemaIdAsync(string name)
        {
            State.Schemas.TryGetValue(name, out var schemaId);

            return Task.FromResult(schemaId);
        }

        public Task<List<Guid>> GetSchemaIdsAsync()
        {
            return Task.FromResult(State.Schemas.Values.ToList());
        }
    }
}
