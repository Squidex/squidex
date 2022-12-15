// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb;

internal sealed class MongoCountCollection : MongoRepositoryBase<MongoCountEntity>
{
    private readonly string collectionName;
    private readonly ILogger log;

    public MongoCountCollection(IMongoDatabase database, ILogger log, string name)
        : base(database)
    {
        this.log = log;

        collectionName = $"{name}_Count";
    }

    protected override string CollectionName()
    {
        return collectionName;
    }

    public async Task<long> GetOrAddAsync(string key, Func<CancellationToken, Task<long>> provider,
        CancellationToken ct)
    {
        var (cachedTotal, isOutdated) = await CountAsync(key, ct);

        // This is our hardcoded limit at the moment. Usually schemas are much smaller anyway.
        if (cachedTotal < 5_000)
        {
            // We always want to have up to date collection sizes for smaller schemas.
            return await RefreshTotalAsync(key, cachedTotal, provider, ct);
        }

        if (isOutdated)
        {
            // If we have a lot of items, the query might be slow and therefore we execute it in the background.
            RefreshSilentAsync(key, cachedTotal, provider).Forget();
        }

        return cachedTotal;
    }

    private async Task RefreshSilentAsync(string key, long cachedCount, Func<CancellationToken, Task<long>> provider)
    {
        try
        {
            await RefreshTotalAsync(key, cachedCount, provider, default);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to update count for collection {collection}.", collectionName);
        }
    }

    private async Task<long> RefreshTotalAsync(string key, long cachedCount, Func<CancellationToken, Task<long>> provider,
        CancellationToken ct)
    {
        var actualCount = await provider(ct);

        if (actualCount == cachedCount)
        {
            return actualCount;
        }

        var now = SystemClock.Instance.GetCurrentInstant();

        await Collection.UpdateOneAsync(x => x.Key == key,
            Update
                .Set(x => x.Key, key)
                .SetOnInsert(x => x.Count, actualCount)
                .SetOnInsert(x => x.Created, now),
            Upsert, ct);

        return actualCount;
    }

    private async Task<(long, bool)> CountAsync(string key,
        CancellationToken ct)
    {
        var entity = await Collection.Find(x => x.Key == key).FirstOrDefaultAsync(ct);

        if (entity != null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            return (entity.Count, now - entity.Created > Duration.FromSeconds(10));
        }

        return (0, true);
    }
}
