// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Squidex.Infrastructure.Queries;

public static class QueryExtensions
{
    public static async Task CreateGeoIndexAsync(this DatabaseFacade database, SqlDialect dialect, string name, string table, string column,
        CancellationToken ct = default)
    {
        var sql = dialect.GeoIndex(name, table, column);
        try
        {
            await database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex) when (dialect.IsDuplicateIndexException(ex, name))
        {
            // NOOP
        }
    }

    public static async Task CreateTextIndexAsync(this DatabaseFacade database, SqlDialect dialect, string name, string table, string column,
        CancellationToken ct = default)
    {
        var prepareSql = dialect.TextIndexPrepare(name);

        if (!string.IsNullOrWhiteSpace(prepareSql))
        {
            try
            {
                await database.ExecuteSqlRawAsync(prepareSql, ct);
            }
            catch (Exception ex) when (dialect.IsDuplicateIndexException(ex, name))
            {
                // NOOP
            }
        }

        var sql = dialect.TextIndex(name, table, column);
        try
        {
            await database.ExecuteSqlRawAsync(sql, ct);
        }
        catch (Exception ex) when (dialect.IsDuplicateIndexException(ex, name))
        {
            // NOOP
        }
    }
}
