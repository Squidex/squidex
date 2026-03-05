// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Hosting;

namespace Squidex.Infrastructure.Queries;

public sealed class SqlDialectInitializer<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IInitializable where TContext : DbContext
{
    public async Task InitializeAsync(CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        if (dbContext is not IDbContextWithDialect withDialect)
        {
            return;
        }

        await withDialect.Dialect.InitializeAsync(dbContext, ct);
    }
}
