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
using Orleans;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasIndex : ICommandMiddleware, ISchemasIndex
    {
        private readonly IGrainFactory grainFactory;

        public SchemasIndex(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task RebuildAsync(Guid appId, Dictionary<string, Guid> schemas)
        {
            return Index(appId).RebuildAsync(schemas);
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var ids = await GetSchemaIdsAsync(appId);

                var schemas =
                    await Task.WhenAll(
                        ids.Select(id => GetSchemaAsync(appId, id, allowDeleted)));

                return schemas.Where(x => x != null).ToList();
            }
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var id = await GetSchemaIdAsync(appId, name);

                if (id == default)
                {
                    return null;
                }

                return await GetSchemaAsync(appId, id, allowDeleted);
            }
        }

        public async Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var schemaEntity = await grainFactory.GetGrain<ISchemaGrain>(id).GetStateAsync();

                if (IsFound(schemaEntity.Value, allowDeleted))
                {
                    return schemaEntity.Value;
                }

                await Index(appId).RemoveSchemaAsync(id);

                return null;
            }
        }

        private async Task<Guid> GetSchemaIdAsync(Guid appId, string name)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                return await Index(appId).GetSchemaIdAsync(name);
            }
        }

        private async Task<List<Guid>> GetSchemaIdsAsync(Guid appId)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                return await Index(appId).GetSchemaIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateSchema createSchema)
            {
                await CreateSchemaAsync(createSchema);
            }

            await next();

            if (context.IsCompleted)
            {
                if (context.Command is DeleteSchema deleteSchema)
                {
                    await DeleteSchemaAsync(deleteSchema);
                }
            }
        }

        private async Task CreateSchemaAsync(CreateSchema createSchema)
        {
            var name = createSchema.Name;

            if (!name.IsSlug())
            {
                return;
            }

            var id = createSchema.SchemaId;

            var appId = createSchema.AppId.Id;

            if (await HasSchemaAsync(name, appId) || !await AddSchemaAsync(appId, name, id))
            {
                var error = new ValidationError("A schema with this name already exists.");

                throw new ValidationException("Cannot create schema.", error);
            }
        }

        private Task<bool> AddSchemaAsync(Guid appId, string schemaName, Guid schemaId)
        {
            return Index(appId).AddSchemaAsync(schemaId, schemaName);
        }

        private async Task DeleteSchemaAsync(DeleteSchema deleteSchema)
        {
            var schemaId = deleteSchema.SchemaId;

            var schema = await grainFactory.GetGrain<ISchemaGrain>(schemaId).GetStateAsync();

            if (IsFound(schema.Value, true))
            {
                await Index(schema.Value.AppId.Id).RemoveSchemaAsync(schemaId);
            }
        }

        private async Task<bool> HasSchemaAsync(string schemaName, Guid appId)
        {
            return await GetSchemaAsync(appId, schemaName) != null;
        }

        private ISchemasByAppIndexGrain Index(Guid appId)
        {
            return grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId);
        }

        private static bool IsFound(ISchemaEntity entity, bool allowDeleted)
        {
            return entity.Version > EtagVersion.Empty && (!entity.IsDeleted || allowDeleted);
        }
    }
}
