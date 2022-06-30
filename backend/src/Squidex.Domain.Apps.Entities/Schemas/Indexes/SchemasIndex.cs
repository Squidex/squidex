// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    public sealed class SchemasIndex : ICommandMiddleware, ISchemasIndex
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private readonly ISchemaRepository schemaRepository;
        private readonly IReplicatedCache schemaCache;
        private readonly IUniqueNamesState uniqueNamesState;

        public SchemasIndex(ISchemaRepository schemaRepository, IReplicatedCache schemaCache, IUniqueNamesState uniqueNamesState)
        {
            this.schemaRepository = schemaRepository;
            this.schemaCache = schemaCache;
            this.uniqueNamesState = uniqueNamesState;
        }

        public async Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("SchemasIndex/GetSchemasAsync"))
            {
                var schemas = await schemaRepository.QueryAllAsync(appId, ct);

                foreach (var schema in schemas)
                {
                    await InvalidateItAsync(appId, schema.Id, schema.SchemaDef.Name);
                }

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
                    if (schemaCache.TryGetValue(cacheKey, out var value) && value is ISchemaEntity cachedSchema)
                    {
                        return cachedSchema;
                    }
                }

                var schema = await schemaRepository.FindAsync(appId, name, ct);

                if (schema != null)
                {
                    await CacheItAsync(schema);
                }

                return schema;
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
                    if (schemaCache.TryGetValue(cacheKey, out var v) && v is ISchemaEntity cachedSchema)
                    {
                        return cachedSchema;
                    }
                }

                var schema = await schemaRepository.FindAsync(appId, id, ct);

                if (schema != null)
                {
                    await CacheItAsync(schema);
                }

                return schema;
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            var command = context.Command;

            if (command is CreateSchema createSchema)
            {
                var token = await CheckSchemaAsync(createSchema);
                try
                {
                    await next(context);
                }
                finally
                {
                    await uniqueNamesState.RemoveReservationAsync(token);
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

        private Task OnCreateAsync(CreateSchema create)
        {
            return InvalidateItAsync(create.AppId.Id, create.SchemaId, create.Name);
        }

        private Task OnDeleteAsync(DeleteSchema delete)
        {
            return InvalidateItAsync(delete.AppId.Id, delete.SchemaId.Id, delete.SchemaId.Name);
        }

        private Task OnUpdateAsync(SchemaUpdateCommand update)
        {
            return InvalidateItAsync(update.AppId.Id, update.SchemaId.Id, update.SchemaId.Name);
        }

        private async Task<string> CheckSchemaAsync(CreateSchema command)
        {
            var existing = await schemaRepository.FindAsync(command.AppId.Id, command.Name);

            if (existing != null && !existing.IsDeleted)
            {
                throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
            }

            var token = await uniqueNamesState.ReserveAsync(command.SchemaId, $"SCHEMA_{command.Name}");

            if (token == null)
            {
                throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
            }

            return token;
        }

        private Task InvalidateItAsync(DomainId appId, DomainId id, string name)
        {
            return schemaCache.RemoveAsync(
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
                schemaCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.Id), schema, CacheDuration),
                schemaCache.AddAsync(GetCacheKey(schema.AppId.Id, schema.SchemaDef.Name), schema, CacheDuration));
        }
    }
}
