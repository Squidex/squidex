// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public static Task CreateGeoIndexAsync<TContext>(this TContext dbContext, string name, string table, string column, ILogger log,
        CancellationToken ct = default)
        where TContext : DbContext, IDbContextWithDialect
    {
        var sqlCreate = dbContext.Dialect.GeoIndex(name, table, column);

        return dbContext.CreateIndexAsync(name, [sqlCreate], log, ct);
    }

    public static Task CreateTextIndexAsync<TContext>(this TContext dbContext, string name, string table, string column, ILogger log,
        CancellationToken ct = default)
        where TContext : DbContext, IDbContextWithDialect
    {
        var sqlPrepare = dbContext.Dialect.TextIndexPrepare(name);
        var sqlCreate = dbContext.Dialect.TextIndex(name, table, column);

        if (string.IsNullOrWhiteSpace(sqlPrepare))
        {
            return dbContext.CreateIndexAsync(name, [sqlCreate], log, ct);
        }

        return dbContext.CreateIndexAsync(name, [sqlPrepare, sqlCreate], log, ct);
    }

    private static async Task CreateIndexAsync<TContext>(this TContext dbContext, string name, string[] statements, ILogger log,
        CancellationToken ct)
        where TContext : DbContext, IDbContextWithDialect
    {
        for (var i = 0; i < statements.Length; i++)
        {
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(statements[i], ct);
            }
            catch (Exception ex) when (dbContext.Dialect.IsDuplicateIndexException(ex, name))
            {
                // NOOP
            }
            catch (Exception ex) when (dbContext.Dialect.IsPrivilegeMissingException(ex))
            {
                // Only the queries that use the index fail without it, therefore the application can still start.
                LogMessages.LogIndexPrivilegesMissing(log, ex, name, string.Join('\n', statements[i..]));
                return;
            }
        }
    }
}
