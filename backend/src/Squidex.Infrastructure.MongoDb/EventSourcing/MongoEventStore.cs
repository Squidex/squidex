// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.EventSourcing;

public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
{
    private readonly IEventNotifier notifier;

    public IMongoCollection<BsonDocument> RawCollection
    {
        get => Database.GetCollection<BsonDocument>(CollectionName());
    }

    public IMongoCollection<MongoEventCommit> TypedCollection
    {
        get => Collection;
    }

    public bool CanUseChangeStreams { get; private set; }

    public MongoEventStore(IMongoDatabase database, IEventNotifier notifier)
        : base(database)
    {
        this.notifier = notifier;
    }

    protected override string CollectionName()
    {
        return "Events2";
    }

    protected override MongoCollectionSettings CollectionSettings()
    {
        return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
    }

    protected override async Task SetupCollectionAsync(IMongoCollection<MongoEventCommit> collection,
        CancellationToken ct)
    {
        await collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<MongoEventCommit>(
                Index
                    .Ascending(x => x.Timestamp)),
            new CreateIndexModel<MongoEventCommit>(
                Index
                    .Ascending(x => x.Timestamp)
                    .Ascending(x => x.EventStream)),
            new CreateIndexModel<MongoEventCommit>(
                Index
                    .Descending(x => x.Timestamp)
                    .Ascending(x => x.EventStream)),
            new CreateIndexModel<MongoEventCommit>(
                Index
                    .Ascending(x => x.EventStream)
                    .Descending(x => x.EventStreamOffset),
                new CreateIndexOptions
                {
                    Unique = true
                })
        }, ct);

        var clusterVersion = await Database.GetMajorVersionAsync(ct);
        var clusteredAsReplica = Database.Client.Cluster.Description.Type == ClusterType.ReplicaSet;

        CanUseChangeStreams = clusteredAsReplica && clusterVersion >= 4;
    }
}
