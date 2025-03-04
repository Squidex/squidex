// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents;

public readonly partial record struct DynamicContextName(DomainId AppId, DomainId SchemaId)
{
    public static bool TryParse(string tableName, out DynamicContextName result)
    {
        result = default;
        if (string.IsNullOrEmpty(tableName))
        {
            return false;
        }

        var match = PrefixRegex().Match(tableName);
        if (!match.Success)
        {
            return false;
        }

        result =
            new DynamicContextName(
                DomainId.Create(match.Groups["AppId"].Value),
                DomainId.Create(match.Groups["SchemaId"].Value));

        return true;
    }

    [GeneratedRegex("^BySchema_(?<AppId>[0-9a-f\\-]{36})_(?<SchemaId>[0-9a-f\\-]{36})__")]
    private static partial Regex PrefixRegex();

    public override readonly string ToString()
    {
        return $"BySchema_{AppId}_{SchemaId}__";
    }
}

public sealed class DynamicTables<TContentContext>(IDbContextNamedFactory<TContentContext> dbContextFactory, SqlDialect dialect)
    where TContentContext : ContentDbContext
{
    private readonly Dictionary<DynamicContextName, Task> dynamicMigrations = new Dictionary<DynamicContextName, Task>();

    public async IAsyncEnumerable<DynamicContextName> GetContextNames(
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync(string.Empty, default);

        var tableNames = await dbContext.Database.SqlQueryRaw<string>(dialect.SelectTables()).ToListAsync(ct);
        var tableResults = new HashSet<DynamicContextName>();

        foreach (var table in tableNames)
        {
            if (DynamicContextName.TryParse(table, out var name) && tableResults.Add(name))
            {
                yield return name;
            }
        }
    }

    public Task<TContentContext> CreateDbContextAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct)
    {
        return CreateDbContextAsync(new DynamicContextName(appId, schemaId), ct);
    }

    public async Task<TContentContext> CreateDbContextAsync(DynamicContextName name,
        CancellationToken ct)
    {
        Guard.NotDefault(name);

        async Task MigrateAsync()
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync(name.ToString(), default);

            await dbContext.MigrateAsync(default);
        }

        Task migration;
        lock (dynamicMigrations)
        {
            if (!dynamicMigrations.TryGetValue(name, out var temp))
            {
                temp = MigrateAsync();
                dynamicMigrations[name] = temp;
            }

            migration = temp;
        }

        await migration;

        return await dbContextFactory.CreateDbContextAsync(name.ToString(), ct);
    }
}
