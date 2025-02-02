// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Squidex.Hosting;

namespace Squidex.Infrastructure.Migrations;

public sealed class DatabaseCreator<TContext>(IDbContextFactory<TContext> dbContextFactory) : IInitializable
    where TContext : DbContext
{
    public int Order => -1000;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);

        if (context.Database.GetService<IDatabaseCreator>() is not RelationalDatabaseCreator relationalDatabaseCreator)
        {
            return;
        }

        await relationalDatabaseCreator.EnsureCreatedAsync(ct);
    }
}
