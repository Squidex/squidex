// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents;

public readonly record struct DynamicContextName(DomainId AppId, DomainId SchemaId);

public sealed class DynamicTables<TContext, TContentContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IDbContextNamedFactory<TContentContext> dbContextNamedFactory)
    where TContext : DbContext where TContentContext : ContentDbContext
{
    private readonly Dictionary<DynamicContextName, Task<string>> cachedMappings = [];

    public async IAsyncEnumerable<DynamicContextName> GetContextNames(
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var tableEntities = await dbContext.Set<EFContentTableEntity>().ToListAsync(ct);

        lock (cachedMappings)
        {
            foreach (var entity in tableEntities)
            {
                var name = new DynamicContextName(entity.AppId, entity.SchemaId);
                if (!cachedMappings.ContainsKey(name))
                {
                    cachedMappings[name] = Task.FromResult(Prefix(entity));
                }
            }
        }

        foreach (var entity in tableEntities)
        {
            yield return new DynamicContextName(entity.AppId, entity.SchemaId);
        }
    }

    public async Task<TContentContext> CreateDbContextAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct)
    {
        var prefix = await EnsureDbContextAsync(appId, schemaId);

        return await dbContextNamedFactory.CreateDbContextAsync(prefix, ct);
    }

    public async Task<TContentContext> CreateDbContextAsync(DynamicContextName name,
        CancellationToken ct)
    {
        var prefix = await EnsureDbContextAsync(name);

        return await dbContextNamedFactory.CreateDbContextAsync(prefix, ct);
    }

    public async Task<string> EnsureDbContextAsync(DomainId appId, DomainId schemaId)
    {
        return await EnsureDbContextAsync(new DynamicContextName(appId, schemaId));
    }

    public async Task<string> EnsureDbContextAsync(DynamicContextName name)
    {
        Guard.NotDefault(name);

        async Task<string> PrepareAsync()
        {
            var prefix = await GetPrefixAsync(name);

            await using var dbContext = await dbContextNamedFactory.CreateDbContextAsync(prefix, default);

            // Make the prefix available as async local variable because migrations cannot use it otherwise.
            TableName.Prefix = prefix;
            await dbContext.Database.MigrateAsync(default);
            await dbContext.DisposeAsync();

            return prefix;
        }

        Task<string> preparation;
        lock (cachedMappings)
        {
            if (!cachedMappings.TryGetValue(name, out var temp))
            {
                temp = PrepareAsync();
                cachedMappings[name] = temp;
            }

            preparation = temp;
        }

        try
        {
            return await preparation;
        }
        catch
        {
            // Do not cache errors forever, otherwise we would not be able to recover.
            lock (cachedMappings)
            {
                if (cachedMappings.TryGetValue(name, out var temp) && ReferenceEquals(temp, preparation))
                {
                    cachedMappings.Remove(name);
                }
            }

            throw;
        }
    }

    private async Task<string> GetPrefixAsync(DynamicContextName name)
    {
        var prefix = string.Empty;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(default);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
        try
        {
            var entity = new EFContentTableEntity { AppId = name.AppId, SchemaId = name.SchemaId };

            await dbContext.Set<EFContentTableEntity>().AddAsync(entity, default);
            await dbContext.SaveChangesAsync(default);

            prefix = Prefix(entity);
        }
        catch
        {
            // Very likely a unique index exception.
        }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

        if (string.IsNullOrWhiteSpace(prefix))
        {
            var existing =
                await dbContext.Set<EFContentTableEntity>().Where(x => x.AppId == name.AppId && x.SchemaId == name.SchemaId)
                    .FirstOrDefaultAsync(default)
                    ?? throw new InvalidOperationException("Cannot resolve mapping table for schema.");

            prefix = Prefix(existing);
        }

        return prefix;
    }

    private static string Prefix(EFContentTableEntity entity)
    {
        return $"__c{entity.Id}_";
    }
}
