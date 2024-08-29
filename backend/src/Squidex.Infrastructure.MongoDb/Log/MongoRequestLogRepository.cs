// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Log;

public sealed class MongoRequestLogRepository : MongoRepositoryBase<MongoRequest>, IRequestLogRepository
{
    private readonly RequestLogStoreOptions options;

    public MongoRequestLogRepository(IMongoDatabase database, IOptions<RequestLogStoreOptions> options)
        : base(database)
    {
        Guard.NotNull(options);

        this.options = options.Value;
    }

    protected override string CollectionName()
    {
        return "RequestLog";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoRequest> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(
        [
            new CreateIndexModel<MongoRequest>(
                Index
                    .Ascending(x => x.Key)
                    .Ascending(x => x.Timestamp)),
            new CreateIndexModel<MongoRequest>(
                Index
                    .Ascending(x => x.Timestamp),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(options.StoreRetentionInDays)
                })
        ], ct);
    }

    public Task InsertManyAsync(IEnumerable<Request> items,
        CancellationToken ct = default)
    {
        Guard.NotNull(items);

        var entities = items.Select(MongoRequest.FromRequest).ToList();

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

    public IAsyncEnumerable<Request> QueryAllAsync(string key, DateTime fromDate, DateTime toDate,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(key);

        var timestampStart = Instant.FromDateTimeUtc(fromDate);
        var timestampEnd = Instant.FromDateTimeUtc(toDate.AddDays(1));

        var find = Collection.Find(x => x.Key == key && x.Timestamp >= timestampStart && x.Timestamp < timestampEnd);

        var documents = find.ToAsyncEnumerable(ct);

        return documents.Select(x => x.ToRequest());
    }
}
