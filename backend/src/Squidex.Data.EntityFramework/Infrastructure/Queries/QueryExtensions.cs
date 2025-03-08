// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;

namespace Squidex.Infrastructure.Queries;

public static class QueryExtensions
{
    public static SqlQueryBuilder Query<T>(this DbContext dbContext, SqlParams? parameters = null)
    {
        if (dbContext is not IDbContextWithDialect withDialect)
        {
            throw new InvalidOperationException("Invalid context.");
        }

        var tableName = dbContext.Model.FindEntityType(typeof(T))?.GetTableName()
            ?? throw new InvalidOperationException("Unknown model.");

        return new SqlQueryBuilder(withDialect.Dialect, tableName, parameters);
    }

    public static async Task CreateGeoIndexAsync<TContext>(this TContext dbContext, string name, string table, string column,
        CancellationToken ct = default)
        where TContext : DbContext, IDbContextWithDialect
    {
        var sql = dbContext.Dialect.GeoIndex(name, table, column);
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex) when (dbContext.Dialect.IsDuplicateIndexException(ex, name))
        {
            // NOOP
        }
    }

    public static async Task CreateTextIndexAsync<TContext>(this TContext dbContext, string name, string table, string column,
        CancellationToken ct = default)
        where TContext : DbContext, IDbContextWithDialect
    {
        var prepareSql = dbContext.Dialect.TextIndexPrepare(name);

        if (!string.IsNullOrWhiteSpace(prepareSql))
        {
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(prepareSql, ct);
            }
            catch (Exception ex) when (dbContext.Dialect.IsDuplicateIndexException(ex, name))
            {
                // NOOP
            }
        }

        var sql = dbContext.Dialect.TextIndex(name, table, column);
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex) when (dbContext.Dialect.IsDuplicateIndexException(ex, name))
        {
            // NOOP
        }
    }
}
