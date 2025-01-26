// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NodaTime;

namespace Squidex.Infrastructure.Log;

public sealed class MongoRequestLogRepository(IMongoDatabase database, IOptions<RequestLogStoreOptions> options)
    : MongoRepositoryBase<MongoRequestEntity>(database), IRequestLogRepository
{
    private readonly RequestLogStoreOptions options = options.Value;

    protected override string CollectionName()
    {
        return "RequestLog";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoRequestEntity> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoRequestEntity>(
                Index
                    .Ascending(x => x.Key)
                    .Ascending(x => x.Timestamp)),
            new CreateIndexModel<MongoRequestEntity>(
                Index
                    .Ascending(x => x.Timestamp),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(options.StoreRetentionInDays),
                }),
        ], ct);
    }

    public Task InsertManyAsync(IEnumerable<Request> items,
        CancellationToken ct = default)
    {
        Guard.NotNull(items);

        var entities = items.Select(MongoRequestEntity.FromRequest).ToList();

        if (entities.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.InsertManyAsync(entities, InsertUnordered, ct);
    }

    public Task DeleteAsync(string key,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        return Collection.DeleteManyAsync(Filter.Eq(x => x.Key, key), ct);
    }

    public IAsyncEnumerable<Request> QueryAllAsync(string key, Instant fromTime, Instant toTime,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        var documents =
            Collection.Find(x =>
                x.Key == key &&
                x.Timestamp >= fromTime &&
                x.Timestamp <= toTime)
            .ToAsyncEnumerable(ct);

        return documents.Select(x => x.ToRequest());
    }
}
