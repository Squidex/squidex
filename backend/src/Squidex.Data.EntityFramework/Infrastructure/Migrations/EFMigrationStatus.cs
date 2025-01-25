// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Hosting;

namespace Squidex.Infrastructure.Migrations;

public sealed class EFMigrationStatus<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IMigrationStatus, IInitializable where TContext : DbContext
{
    private const int DefaultId = 1;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        try
        {
            var newEntry = new EFMigrationEntity { Id = DefaultId };

            await dbContext.Set<EFMigrationEntity>().AddAsync(newEntry, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
        }
    }

    public async Task<int> GetVersionAsync(
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var entity =
            await dbContext.Set<EFMigrationEntity>()
                .Where(x => x.Id == DefaultId).FirstOrDefaultAsync(ct);

        return entity?.Version ?? 0;
    }

    public async Task<bool> TryLockAsync(
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var updateCount =
            await dbContext.Set<EFMigrationEntity>()
                .Where(x => x.Id == DefaultId && !x.IsLocked)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsLocked, true), ct);

        return updateCount == 1;
    }

    public async Task CompleteAsync(int newVersion,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFMigrationEntity>()
            .Where(x => x.Id == DefaultId)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Version, newVersion), ct);
    }

    public async Task UnlockAsync(
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFMigrationEntity>()
            .Where(x => x.Id == DefaultId && x.IsLocked)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsLocked, false), ct);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
