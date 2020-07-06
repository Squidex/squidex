// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public Task RebuildAsync(DomainId appId, Dictionary<string, DomainId> schemas)
        {
            return Index(appId).RebuildAsync(schemas);
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var ids = await GetSchemaIdsAsync(appId);

                var schemas =
                    await Task.WhenAll(
                        ids.Select(id => GetSchemaAsync(appId, id, allowDeleted)));

                return schemas.NotNull().ToList();
            }
        }

        public async Task<ISchemaEntity?> GetSchemaByNameAsync(DomainId appId, string name, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var id = await GetSchemaIdAsync(appId, name);

                if (id == DomainId.Empty)
                {
                    return null;
                }

                return await GetSchemaAsync(appId, id, allowDeleted);
            }
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool allowDeleted = false)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var schema = await GetSchemaInternalAsync(appId, id);

                if (IsFound(schema, allowDeleted))
                {
                    return schema;
                }

                return null;
            }
        }

        private async Task<DomainId> GetSchemaIdAsync(DomainId appId, string name)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                return await Index(appId).GetIdAsync(name);
            }
        }

        private async Task<List<DomainId>> GetSchemaIdsAsync(DomainId appId)
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
            var schema = await GetSchemaInternalAsync(commmand.AppId.Id, commmand.SchemaId.Id);

            if (IsFound(schema, true))
            {
                await Index(schema.AppId.Id).RemoveAsync(schema.Id);
            }
        }

        private async Task<ISchemaEntity> GetSchemaInternalAsync(DomainId appId, DomainId id)
        {
            var key = DomainId.Combine(appId, id).ToString();

            var rule = await grainFactory.GetGrain<ISchemaGrain>(key).GetStateAsync();

            return rule.Value;
        }

        private ISchemasByAppIndexGrain Index(DomainId appId)
        {
            return grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId.ToString());
        }

        private static bool IsFound(ISchemaEntity entity, bool allowDeleted)
        {
            return entity.Version > EtagVersion.Empty && (!entity.IsDeleted || allowDeleted);
        }
    }
}
