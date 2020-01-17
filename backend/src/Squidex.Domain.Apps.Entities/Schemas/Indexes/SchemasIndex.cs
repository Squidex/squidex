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
            Guard.NotNull(grainFactory);

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

        public async Task<ISchemaEntity?> GetSchemaByNameAsync(Guid appId, string name, bool allowDeleted = false)
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

        public async Task<ISchemaEntity?> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var schema = await grainFactory.GetGrain<ISchemaGrain>(id).GetStateAsync();

                if (IsFound(schema.Value, allowDeleted))
                {
                    return schema.Value;
                }

                return null;
            }
        }

        private async Task<Guid> GetSchemaIdAsync(Guid appId, string name)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                return await Index(appId).GetIdAsync(name);
            }
        }

        private async Task<List<Guid>> GetSchemaIdsAsync(Guid appId)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                return await Index(appId).GetIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (context.Command is CreateSchema createSchema)
            {
                var index = Index(createSchema.AppId.Id);

                var token = await CheckSchemaAsync(index, createSchema);

                try
                {
                    await next(context);
                }
                finally
                {
                    if (token != null)
                    {
                        if (context.IsCompleted)
                        {
                            await index.AddAsync(token);
                        }
                        else
                        {
                            await index.RemoveReservationAsync(token);
                        }
                    }
                }
            }
            else
            {
                await next(context);

                if (context.IsCompleted)
                {
                    if (context.Command is DeleteSchema deleteSchema)
                    {
                        await DeleteSchemaAsync(deleteSchema);
                    }
                }
            }
        }

        private static async Task<string?> CheckSchemaAsync(ISchemasByAppIndexGrain index, CreateSchema command)
        {
            var name = command.Name;

            if (name.IsSlug())
            {
                var token = await index.ReserveAsync(command.SchemaId, name);

                if (token == null)
                {
                    var error = new ValidationError("A schema with this name already exists.");

                    throw new ValidationException("Cannot create schema.", error);
                }

                return token;
            }

            return null;
        }

        private async Task DeleteSchemaAsync(DeleteSchema commmand)
        {
            var schemaId = commmand.SchemaId;

            var schema = await grainFactory.GetGrain<ISchemaGrain>(schemaId).GetStateAsync();

            if (IsFound(schema.Value, true))
            {
                await Index(schema.Value.AppId.Id).RemoveAsync(schemaId);
            }
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
