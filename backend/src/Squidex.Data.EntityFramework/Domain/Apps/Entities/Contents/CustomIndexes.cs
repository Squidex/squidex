// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents;

internal static partial class CustomIndexes
{
    // Fields inside JSON columns are indexed with JSON functions, the other columns must be scalar.
    private static readonly HashSet<string> IndexableColumns =
    [
        nameof(EFContentEntity.Created),
        nameof(EFContentEntity.CreatedBy),
        nameof(EFContentEntity.Id),
        nameof(EFContentEntity.IsDeleted),
        nameof(EFContentEntity.LastModified),
        nameof(EFContentEntity.LastModifiedBy),
        nameof(EFContentEntity.NewStatus),
        nameof(EFContentEntity.ScheduledAt),
        nameof(EFContentEntity.Status),
        nameof(EFContentEntity.Version),
    ];

    [GeneratedRegex("^[a-zA-Z0-9_\\-]+(\\.[a-zA-Z0-9_\\-]+)*$")]
    private static partial Regex FieldNameRegex();

    public static async Task CreateCustomIndexAsync<T>(this ContentDbContext dbContext, IndexDefinition index,
        CancellationToken ct) where T : EFContentEntity
    {
        var table = GetTableName<T>(dbContext);

        var queryBuilder = dbContext.ContentQuery<T>();

        var indexKeys = new List<SqlIndexKey>();
        foreach (var field in index)
        {
            if (!FieldNameRegex().IsMatch(field.Name))
            {
                throw new DomainException($"Invalid index field '{field.Name}'.");
            }

            var fieldPath = queryBuilder.Visit(field.Name);
            var fieldIsJson = queryBuilder.IsJsonPath(fieldPath);

            if (!fieldIsJson && (fieldPath.Count > 1 || !IndexableColumns.Contains(fieldPath[0])))
            {
                throw new DomainException($"Field '{field.Name}' cannot be indexed.");
            }

            indexKeys.AddRange(dbContext.Dialect.IndexKeys(fieldPath, field.Order, fieldIsJson));
        }

        if (indexKeys.Count == 0)
        {
            throw new DomainException("Index does not contain a field.");
        }

        var indexName = GetIndexName(table, index.ToName());
        var indexesExisting = await dbContext.Dialect.QueryIndexesAsync(dbContext, table, ct);

        if (indexesExisting.Any(x => x.Name == indexName))
        {
            return;
        }

        foreach (var sql in dbContext.Dialect.CreateIndex(indexName, table, indexKeys, Describe(index)))
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    public static async Task<List<IndexDefinition>> GetCustomIndexesAsync<T>(this ContentDbContext dbContext,
        CancellationToken ct) where T : EFContentEntity
    {
        var table = GetTableName<T>(dbContext);

        var result = new List<IndexDefinition>();
        foreach (var index in await dbContext.Dialect.QueryIndexesAsync(dbContext, table, ct))
        {
            if (!index.Name.StartsWith(GetIndexPrefix(table), StringComparison.Ordinal))
            {
                continue;
            }

            if (TryParse(index.Description, out var definition))
            {
                result.Add(definition);
            }
        }

        return result;
    }

    public static async Task DropCustomIndexAsync<T>(this ContentDbContext dbContext, string name,
        CancellationToken ct) where T : EFContentEntity
    {
        var table = GetTableName<T>(dbContext);

        var indexName = GetIndexName(table, name);
        var indexesExisting = await dbContext.Dialect.QueryIndexesAsync(dbContext, table, ct);

        if (!indexesExisting.Any(x => x.Name == indexName))
        {
            return;
        }

        foreach (var sql in dbContext.Dialect.DropIndex(indexName, table))
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }
    }

    private static string GetTableName<T>(DbContext dbContext)
    {
        return dbContext.Model.FindEntityType(typeof(T))?.GetTableName()
            ?? throw new InvalidOperationException("Unknown model.");
    }

    private static string GetIndexPrefix(string table)
    {
        return $"IX_{table}_cx_";
    }

    private static string GetIndexName(string table, string name)
    {
        // Index names are limited to 63 characters in Postgres, therefore the readable name is only stored as description.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..16].ToLowerInvariant();

        return $"{GetIndexPrefix(table)}{hash}";
    }

    private static string Describe(IndexDefinition index)
    {
        return string.Join(',', index.Select(x => $"{x.Name} {(x.Order == SortOrder.Ascending ? "asc" : "desc")}"));
    }

    private static bool TryParse(string? description, out IndexDefinition index)
    {
        index = [];

        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        foreach (var part in description.Split(','))
        {
            var partTokens = part.Split(' ');
            if (partTokens.Length != 2)
            {
                return false;
            }

            var partOrder = partTokens[1] == "desc" ? SortOrder.Descending : SortOrder.Ascending;

            index.Add(new IndexField(partTokens[0], partOrder));
        }

        return index.Count > 0;
    }
}
