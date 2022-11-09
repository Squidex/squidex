// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.History;

public sealed class MongoHistoryEventRepository : MongoRepositoryBase<HistoryEvent>, IHistoryEventRepository, IDeleter
{
    static MongoHistoryEventRepository()
    {
        BsonClassMap.RegisterClassMap<HistoryEvent>(cm =>
        {
            cm.AutoMap();

            cm.MapProperty(x => x.OwnerId)
                .SetElementName("AppId");

            cm.MapProperty(x => x.EventType)
                .SetElementName("Message");
        });
    }

    public MongoHistoryEventRepository(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "Projections_History";
    }

    protected override Task SetupCollectionAsync(IMongoCollection<HistoryEvent> collection,
        CancellationToken ct)
    {
        return collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<HistoryEvent>(
                Index
                    .Ascending(x => x.OwnerId)
                    .Ascending(x => x.Channel)
                    .Descending(x => x.Created)
                    .Descending(x => x.Version)),
            new CreateIndexModel<HistoryEvent>(
                Index
                    .Ascending(x => x.OwnerId)
                    .Descending(x => x.Created)
                    .Descending(x => x.Version))
        }, ct);
    }

    async Task IDeleter.DeleteAppAsync(IAppEntity app,
        CancellationToken ct)
    {
        await Collection.DeleteManyAsync(Filter.Eq(x => x.OwnerId, app.Id), ct);
    }

    public async Task<IReadOnlyList<HistoryEvent>> QueryByChannelAsync(DomainId ownerId, string channelPrefix, int count,
        CancellationToken ct = default)
    {
        var find =
            !string.IsNullOrWhiteSpace(channelPrefix) ?
                Collection.Find(x => x.OwnerId == ownerId && x.Channel == channelPrefix) :
                Collection.Find(x => x.OwnerId == ownerId);

        return await find.SortByDescending(x => x.Created).ThenByDescending(x => x.Version).Limit(count).ToListAsync(ct);
    }

    public Task InsertManyAsync(IEnumerable<HistoryEvent> historyEvents,
        CancellationToken ct = default)
    {
        var writes = historyEvents
            .Select(x =>
                new ReplaceOneModel<HistoryEvent>(Filter.Eq(y => y.Id, x.Id), x)
                {
                    IsUpsert = true
                })
            .ToList();

        if (writes.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
    }
}
