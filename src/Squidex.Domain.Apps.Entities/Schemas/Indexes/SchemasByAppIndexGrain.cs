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
    public sealed class SchemasByAppIndexGrain : GrainOfGuid, ISchemasByAppIndexGrain
    {
        private readonly IGrainState<GrainState> state;

        [CollectionName("Index_SchemasByApp")]
        public sealed class GrainState
        {
            public Dictionary<string, Guid> Schemas { get; set; } = new Dictionary<string, Guid>();
        }

        public SchemasByAppIndexGrain(IGrainState<GrainState> state)
        {
            Guard.NotNull(state, nameof(state));

            this.state = state;
        }

        public Task RebuildAsync(Dictionary<string, Guid> schemas)
        {
            state.Value = new GrainState { Schemas = schemas };

            return state.WriteAsync();
        }

        public Task AddSchemaAsync(Guid schemaId, string name)
        {
            state.Value.Schemas[name] = schemaId;

            return state.WriteAsync();
        }

        public Task RemoveSchemaAsync(Guid schemaId)
        {
            state.Value.Schemas.Remove(state.Value.Schemas.FirstOrDefault(x => x.Value == schemaId).Key ?? string.Empty);

            return state.WriteAsync();
        }

        public Task<Guid> GetSchemaIdAsync(string name)
        {
            state.Value.Schemas.TryGetValue(name, out var schemaId);

            return Task.FromResult(schemaId);
        }

        public Task<List<Guid>> GetSchemaIdsAsync()
        {
            return Task.FromResult(state.Value.Schemas.Values.ToList());
        }
    }
}
