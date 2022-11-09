// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.UsageTracking;

public sealed class MongoUsageRepository : MongoRepositoryBase<MongoUsage>, IUsageRepository
{
    public MongoUsageRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "UsagesV2";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoUsage> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoUsage>(
                Index
                    .Ascending(x => x.Key)
                    .Ascending(x => x.Category)
                    .Ascending(x => x.Date)),
            cancellationToken: ct);
    }

    public Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNull(key);

        return Collection.DeleteManyAsync(x => x.Key == key, ct);
    }

    public Task DeleteByKeyPatternAsync(string pattern,
        CancellationToken ct = default)
    {
        Guard.NotNull(pattern);

        return Collection.DeleteManyAsync(Filter.Regex(x => x.Key, new BsonRegularExpression(pattern)), ct);
    }

    public async Task TrackUsagesAsync(UsageUpdate update,
        CancellationToken ct = default)
    {
        Guard.NotNull(update);

        if (update.Counters.Count > 0)
        {
            var (filter, updateStatement) = CreateOperation(update);

            await Collection.UpdateOneAsync(filter, updateStatement, Upsert, ct);
        }
    }

    public async Task TrackUsagesAsync(UsageUpdate[] updates,
        CancellationToken ct = default)
    {
        Guard.NotNull(updates);

        if (updates.Length == 1)
        {
            await TrackUsagesAsync(updates[0], ct);
        }
        else if (updates.Length > 0)
        {
            var writes = new List<WriteModel<MongoUsage>>();

            foreach (var update in updates)
            {
                if (update.Counters.Count > 0)
                {
                    var (filter, updateStatement) = CreateOperation(update);

                    writes.Add(new UpdateOneModel<MongoUsage>(filter, updateStatement) { IsUpsert = true });
                }
            }

            await Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }
    }

    private static (FilterDefinition<MongoUsage>, UpdateDefinition<MongoUsage>) CreateOperation(UsageUpdate usageUpdate)
    {
        var id = $"{usageUpdate.Key}_{usageUpdate.Date:yyyy-MM-dd}_{usageUpdate.Category}";

        var update = Update
            .SetOnInsert(x => x.Key, usageUpdate.Key)
            .SetOnInsert(x => x.Date, usageUpdate.Date)
            .SetOnInsert(x => x.Category, usageUpdate.Category);

        foreach (var (key, value) in usageUpdate.Counters)
        {
            update = update.Inc($"Counters.{key}", value);
        }

        var filter = Filter.Eq(x => x.Id, id);

        return (filter, update);
    }

    public async Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        var entities = await Collection.Find(x => x.Key == key && x.Date >= fromDate && x.Date <= toDate).ToListAsync(ct);

        return entities.Select(x => new StoredUsage(x.Category, x.Date, x.Counters)).ToList();
    }
}
