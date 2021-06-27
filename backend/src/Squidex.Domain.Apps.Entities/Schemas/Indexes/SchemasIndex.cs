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
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasIndex : ICommandMiddleware, ISchemasIndex
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly IGrainFactory grainFactory;
        private readonly IReplicatedCache grainCache;

        public SchemasIndex(IGrainFactory grainFactory, IReplicatedCache grainCache)
        {
            this.grainFactory = grainFactory;
            this.grainCache = grainCache;
        }

        public Task RebuildAsync(DomainId appId, Dictionary<string, DomainId> schemas)
        {
            return Index(appId).RebuildAsync(schemas);
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var ids = await GetSchemaIdsAsync(appId);

                var schemas =
                    await Task.WhenAll(
                        ids.Select(id => GetSchemaAsync(appId, id, false)));

                return schemas.NotNull().ToList();
            }
        }

        public async Task<ISchemaEntity?> GetSchemaByNameAsync(DomainId appId, string name, bool canCache)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var cacheKey = GetCacheKey(appId, name);

                if (canCache)
                {
                    if (grainCache.TryGetValue(cacheKey, out var v) && v is ISchemaEntity cachedSchema)
                    {
                        return cachedSchema;
                    }
                }

                var id = await GetSchemaIdAsync(appId, name);

                if (id == DomainId.Empty)
                {
                    return null;
                }

                return await GetSchemaAsync(appId, id, canCache);
            }
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache)
        {
            using (Profiler.TraceMethod<SchemasIndex>())
            {
                var cacheKey = GetCacheKey(appId, id);

                if (canCache)
                {
                    if (grainCache.TryGetValue(cacheKey, out var v) && v is ISchemaEntity cachedSchema)
                    {
                        return cachedSchema;
                    }
                }

                var schema = await GetSchemaCoreAsync(DomainId.Combine(appId, id));

                if (schema != null)
                {
                    await CacheItAsync(schema);
                }

                return schema;
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

                if (context.IsCompleted && context.Command is SchemaCommand schemaCommand)
                {
                    var schema = context.PlainResult as ISchemaEntity;

                    if (schema == null)
                    {
                        schema = await GetSchemaCoreAsync(schemaCommand.AggregateId, true);
                    }

                    if (schema != null)
                    {
                        await InvalidateItAsync(schema);

                        if (context.Command is DeleteSchema)
                        {
                            await DeleteSchemaAsync(schema);
                        }
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
                    throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
                }

                return token;
            }

            return null;
        }

        private Task DeleteSchemaAsync(ISchemaEntity schema)
        {
            return Index(schema.AppId.Id).RemoveAsync(schema.Id);
        }

        private ISchemasByAppIndexGrain Index(DomainId appId)
        {
            return grainFactory.GetGrain<ISchemasByAppIndexGrain>(appId.ToString());
        }

        private async Task<ISchemaEntity?> GetSchemaCoreAsync(DomainId id, bool allowDeleted = false)
        {
            var schema = (await grainFactory.GetGrain<ISchemaGrain>(id.ToString()).GetStateAsync()).Value;

            if (schema.Version <= EtagVersion.Empty || (schema.IsDeleted && !allowDeleted))
            {
                return null;
            }

            return schema;
        }

        private static string GetCacheKey(DomainId appId, string name)
        {
            return $"{typeof(SchemasIndex)}_Schemas_Name_{appId}_{name}";
        }

        private static string GetCacheKey(DomainId appId, DomainId id)
        {
            return $"{typeof(SchemasIndex)}_Schemas_Id_{appId}_{id}";
        }

        private Task InvalidateItAsync(ISchemaEntity schema)
        {
            return grainCache.RemoveAsync(
                GetCacheKey(schema.AppId.Id, schema.Id),
                GetCacheKey(schema.AppId.Id, schema.SchemaDef.Name));
        }

        private Task CacheItAsync(ISchemaEntity schema)
        {
            return Task.WhenAll(
                grainCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.Id), schema, CacheDuration),
                grainCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.SchemaDef.Name), schema, CacheDuration));
        }
    }
}
