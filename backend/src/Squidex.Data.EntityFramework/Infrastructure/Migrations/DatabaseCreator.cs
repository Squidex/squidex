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

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Migrations;

public sealed class DatabaseCreator<TContext>(IDbContextFactory<TContext> dbContextFactory) : IInitializable
    where TContext : DbContext
{
    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(30);

    public int Order => -1000;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        using var cts = new CancellationTokenSource(WaitTime);
        while (!await dbContext.Database.CanConnectAsync(cts.Token))
        {
            await Task.Delay(100, cts.Token);
        }

        if (dbContext.Database.GetService<IDatabaseCreator>() is not RelationalDatabaseCreator relationalDatabaseCreator)
        {
            return;
        }

        await relationalDatabaseCreator.EnsureCreatedAsync(ct);
    }
}
