// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

public sealed class SchemasIndex : ICommandMiddleware, ISchemasIndex
{
    private readonly ISchemaRepository schemaRepository;
    private readonly IReplicatedCache schemaCache;
    private readonly IPersistenceFactory<NameReservationState.State> persistenceFactory;
    private readonly SchemaCacheOptions options;

    public SchemasIndex(ISchemaRepository schemaRepository, IReplicatedCache schemaCache,
        IPersistenceFactory<NameReservationState.State> persistenceFactory,
        IOptions<SchemaCacheOptions> options)
    {
        this.schemaRepository = schemaRepository;
        this.schemaCache = schemaCache;
        this.persistenceFactory = persistenceFactory;
        this.options = options.Value;
    }

    public async Task<List<Schema>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("SchemasIndex/GetSchemasAsync"))
        {
            activity?.SetTag("appId", appId);

            var schemas = await schemaRepository.QueryAllAsync(appId, ct);

            return await schemas.Where(IsValid).SelectAsync(PrepareAsync);
        }
    }

    public async Task<Schema?> GetSchemaAsync(DomainId appId, string name, bool canCache,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaByNameAsync"))
        {
            activity?.SetTag("appId", appId);
            activity?.SetTag("schemaName", name);

            var cacheKey = GetCacheKey(appId, name);

            if (canCache)
            {
                if (schemaCache.TryGetValue(cacheKey, out var value) && value is Schema cachedSchema)
                {
                    return cachedSchema;
                }
            }

            var schema = await schemaRepository.FindAsync(appId, name, ct);

            if (schema == null || !IsValid(schema))
            {
                return null;
            }

            return await PrepareAsync(schema);
        }
    }

    public async Task<Schema?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("SchemasIndex/GetSchemaAsync"))
        {
            activity?.SetTag("appId", appId);
            activity?.SetTag("schemaId", id);

            var cacheKey = GetCacheKey(appId, id);

            if (canCache)
            {
                if (schemaCache.TryGetValue(cacheKey, out var v) && v is Schema cachedSchema)
                {
                    return cachedSchema;
                }
            }

            var schema = await schemaRepository.FindAsync(appId, id, ct);

            if (schema == null || !IsValid(schema))
            {
                return null;
            }

            return await PrepareAsync(schema);
        }
    }

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        var command = context.Command;

        if (command is CreateSchema createSchema)
        {
            var schemaNames = await GetNamesAsync(createSchema.AppId.Id, ct);
            var schemaTokens = await CheckSchemaAsync(createSchema, schemaNames, ct);
            try
            {
                await next(context, ct);
            }
            finally
            {
                // Always remove the reservation and therefore do not pass over cancellation token.
                await schemaNames.RemoveReservationAsync(schemaTokens, default);
            }
        }
        else
        {
            await next(context, ct);
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
                case SchemaCommand update:
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

    private Task OnUpdateAsync(SchemaCommand update)
    {
        return InvalidateItAsync(update.AppId.Id, update.SchemaId.Id, update.SchemaId.Name);
    }

    private async Task<string> CheckSchemaAsync(CreateSchema command, NameReservationState names,
        CancellationToken ct)
    {
        var existing = await schemaRepository.FindAsync(command.AppId.Id, command.Name, ct);

        if (existing is { IsDeleted: false })
        {
            throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
        }

        var token = await names.ReserveAsync(command.SchemaId, command.Name, ct);

        if (token == null)
        {
            throw new ValidationException(T.Get("schemas.nameAlreadyExists"));
        }

        return token;
    }

    private async Task<NameReservationState> GetNamesAsync(DomainId appId,
        CancellationToken ct)
    {
        var state = new NameReservationState(persistenceFactory, $"{appId}_Schemas");

        await state.LoadAsync(ct);

        return state;
    }

    private static string GetCacheKey(DomainId appId, string name)
    {
        return $"{typeof(SchemasIndex)}_Schemas_Name_{appId}_{name}";
    }

    private static string GetCacheKey(DomainId appId, DomainId id)
    {
        return $"{typeof(SchemasIndex)}_Schemas_Id_{appId}_{id}";
    }

    private static bool IsValid(Schema? schema)
    {
        return schema is { Version: > EtagVersion.Empty, IsDeleted: false };
    }

    private async Task<Schema> PrepareAsync(Schema schema)
    {
        // Run some fallback migrations.
        schema = FieldNames.Migrate(schema);

        if (options.CacheDuration <= TimeSpan.Zero)
        {
            return schema;
        }

        // Do not use cancellation here as we already so far.
        await schemaCache.AddAsync(
        [
            new KeyValuePair<string, object?>(GetCacheKey(schema.AppId.Id, schema.Id), schema),
            new KeyValuePair<string, object?>(GetCacheKey(schema.AppId.Id, schema.Name), schema),
        ], options.CacheDuration);

        return schema;
    }

    private Task InvalidateItAsync(DomainId appId, DomainId id, string name)
    {
        if (options.CacheDuration <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        // Do not use cancellation here as we already so far.
        return schemaCache.RemoveAsync(
        [
            GetCacheKey(appId, id),
            GetCacheKey(appId, name)
        ]);
    }
}
