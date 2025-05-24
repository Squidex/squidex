// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.History;

public sealed class EFHistoryEventRepository<TContext>(IDbContextFactory<TContext> dbContextFactory)
    : IHistoryEventRepository, IDeleter where TContext : DbContext
{
    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<HistoryEvent>().Where(x => x.OwnerId == app.Id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<HistoryEvent>()
            .ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId ownerId, string? channel, int count,
        CancellationToken ct = default)
    {
        await using var dbContext = await CreateDbContextAsync(ct);

        var query = dbContext.Set<HistoryEvent>().Where(x => x.OwnerId == ownerId);
        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(x => x.Channel == channel);
        }

        query = query
            .OrderByDescending(x => x.Created)
            .ThenByDescending(x => x.Version)
            .Take(count);

        var result = await query.ToListAsync(ct);

        return result;
    }

    public async Task InsertManyAsync(IEnumerable<HistoryEvent> historyEvents,
        CancellationToken ct = default)
    {
        var entities = historyEvents.ToList();
        if (entities.Count == 0)
        {
            return;
        }

        await using var dbContext = await CreateDbContextAsync(ct);
        await dbContext.BulkUpsertAsync(entities, ct);
    }

    private Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
