// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Assets;

internal static class Extensions
{
    public static AssetSqlQueryBuilder AssetQuery<T>(this DbContext dbContext, SqlParams? parameters = null)
    {
        if (dbContext is not IDbContextWithDialect withDialect)
        {
            throw new InvalidOperationException("Invalid context.");
        }

        var tableName = dbContext.Model.FindEntityType(typeof(T))?.GetTableName()
            ?? throw new InvalidOperationException("Unknown model.");

        return new AssetSqlQueryBuilder(withDialect.Dialect, tableName, parameters);
    }
}
