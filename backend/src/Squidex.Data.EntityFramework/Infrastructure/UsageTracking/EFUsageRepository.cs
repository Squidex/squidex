// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;

namespace Squidex.Infrastructure.UsageTracking;

public class EFUsageRepository<TContext>(IDbContextFactory<TContext> dbContextFactory) : IUsageRepository where TContext : DbContext
{
    public async Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await CreateDbContextAsync(ct);

        await dbContext.Set<EFUsageCounterEntity>().Where(x => x.Key == key)
            .ExecuteDeleteAsync(ct);
    }

    public Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public async Task TrackUsagesAsync(UsageUpdate[] updates,
        CancellationToken ct = default)
    {
        Guard.NotNull(updates);

        await using var dbContext = await CreateDbContextAsync(ct);

        var set = dbContext.Set<EFUsageCounterEntity>();

        foreach (var update in updates)
        {
            foreach (var (counterKey, counterValue) in update.Counters)
            {
                var date = update.Date.ToDateTime(default, DateTimeKind.Utc);

                var entity = new EFUsageCounterEntity
                {
                    Key = update.Key,
                    Category = update.Category,
                    CounterKey = counterKey,
                    CounterValue = counterValue,
                    Date = date,
                };

                try
                {
                    await set.AddAsync(entity, ct);
                    await dbContext.SaveChangesAsync(ct);
                }
                catch
                {
                    var updateQuery = set.Where(existing =>
                        existing.Key == update.Key &&
                        existing.Category == update.Category &&
                        existing.CounterKey == counterKey &&
                        existing.Date == date);

                    await updateQuery.ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.CounterValue, x => x.CounterValue + counterValue),
                        ct);
                }
            }
        }
    }

    public async Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        await using var dbContext = await CreateDbContextAsync(ct);

        var dateTimeFrom = fromDate.ToDateTime(default, DateTimeKind.Utc);
        var dateTimeTo = toDate.ToDateTime(default, DateTimeKind.Utc);

        var entities =
            await dbContext.Set<EFUsageCounterEntity>()
                .Where(x => x.Key == key)
                .Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo)
                .ToListAsync(ct);

        var result = entities
            .GroupBy(x => new { x.Date, x.Category })
            .Select(group =>
            {
                var counters = new Counters();
                foreach (var item in group)
                {
                    counters[item.CounterKey] = item.CounterValue;
                }

                return new StoredUsage(group.Key.Category, group.Key.Date.ToDateOnly(), counters);
            })
            .ToList();

        return result;
    }

    protected Task<TContext> CreateDbContextAsync(CancellationToken ct)
    {
        return dbContextFactory.CreateDbContextAsync(ct);
    }
}
