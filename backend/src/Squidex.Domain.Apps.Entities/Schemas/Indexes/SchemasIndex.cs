// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

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

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemasAsync"))
            {
                var ids = await GetSchemaIdsAsync(appId);

                var schemas =
                    await Task.WhenAll(
                        ids.Select(id => GetSchemaAsync(appId, id, false, ct)));

                return schemas.NotNull().ToList();
            }
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaByNameAsync"))
            {
                var cacheKey = GetCacheKey(appId, name);

                if (canCache)
                {
                    if (grainCache.TryGetValue(cacheKey, out var value) && value is ISchemaEntity cachedSchema)
                    {
                        return cachedSchema;
                    }
                }

                var id = await GetSchemaIdAsync(appId, name);

                if (id == DomainId.Empty)
                {
                    return null;
                }

                return await GetSchemaAsync(appId, id, canCache, ct);
            }
        }

        public async Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaAsync"))
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
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaIdAsync"))
            {
                return await Cache(appId).GetSchemaIdAsync(name);
            }
        }

        private async Task<IReadOnlyCollection<DomainId>> GetSchemaIdsAsync(DomainId appId)
        {
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaIdsAsync"))
            {
                return await Cache(appId).GetSchemaIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var command = context.Command;

            if (command is CreateSchema createSchema)
            {
                var cache = Cache(createSchema.AppId.Id);

                var token = await CheckSchemaAsync(cache, createSchema);
                try
                {
                    await next(context);
                }
                finally
                {
                    await cache.RemoveReservationAsync(token);
                }
            }
            else
            {
                await next(context);
            }

            if (context.IsCompleted)
            {
                switch (command)
                {
                    case CreateSchema create:
                        await OnCreateAsync(create);
                        break;
                    case DeleteSchema delete:
                        await OnDeleteAsync(delete);
                        break;
                    case SchemaUpdateCommand update:
                        await OnUpdateAsync(update);
                        break;
                }
            }
        }

        private async Task OnCreateAsync(CreateSchema create)
        {
            await InvalidateItAsync(create.AppId.Id, create.SchemaId, create.Name);

            await Cache(create.AppId.Id).AddAsync(create.SchemaId, create.Name);
        }

        private async Task OnDeleteAsync(DeleteSchema delete)
        {
            await InvalidateItAsync(delete.AppId.Id, delete.SchemaId.Id, delete.SchemaId.Name);

            await Cache(delete.AppId.Id).RemoveAsync(delete.SchemaId.Id);
        }

        private async Task OnUpdateAsync(SchemaUpdateCommand update)
        {
            await InvalidateItAsync(update.AppId.Id, update.SchemaId.Id, update.SchemaId.Name);
        }

        private async Task<string?> CheckSchemaAsync(ISchemasCacheGrain cache, CreateSchema command)
        {
            var token = await cache.ReserveAsync(command.SchemaId, command.Name);

            if (token == null)
            {
                throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
            }

            try
            {
                var existingId = await GetSchemaIdAsync(command.AppId.Id, command.Name);

                if (existingId != default)
                {
                    throw new ValidationException(T.Get("apps.nameAlreadyExists"));
                }
            }
            catch
            {
                // Catch our own exception, juist in case something went wrong before.
                await cache.RemoveReservationAsync(token);
                throw;
            }

            return token;
        }

        private ISchemasCacheGrain Cache(DomainId appId)
        {
            return grainFactory.GetGrain<ISchemasCacheGrain>(appId.ToString());
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

        private Task InvalidateItAsync(DomainId appId, DomainId id, string name)
        {
            return grainCache.RemoveAsync(
                GetCacheKey(appId, id),
                GetCacheKey(appId, name));
        }

        private static string GetCacheKey(DomainId appId, string name)
        {
            return $"{typeof(SchemasIndex)}_Schemas_Name_{appId}_{name}";
        }

        private static string GetCacheKey(DomainId appId, DomainId id)
        {
            return $"{typeof(SchemasIndex)}_Schemas_Id_{appId}_{id}";
        }

        private Task CacheItAsync(ISchemaEntity schema)
        {
            return Task.WhenAll(
                grainCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.Id), schema, CacheDuration),
                grainCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.SchemaDef.Name), schema, CacheDuration));
        }
    }
}
