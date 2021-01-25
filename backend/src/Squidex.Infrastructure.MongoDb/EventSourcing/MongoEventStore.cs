﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.EventSourcing
{
    public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        private static readonly FieldDefinition<MongoEventCommit, BsonTimestamp> TimestampField = FieldBuilder.Build(x => x.Timestamp);
        private static readonly FieldDefinition<MongoEventCommit, long> EventsCountField = FieldBuilder.Build(x => x.EventsCount);
        private static readonly FieldDefinition<MongoEventCommit, long> EventStreamOffsetField = FieldBuilder.Build(x => x.EventStreamOffset);
        private static readonly FieldDefinition<MongoEventCommit, string> EventStreamField = FieldBuilder.Build(x => x.EventStream);
        private readonly IEventNotifier notifier;

        public IMongoCollection<BsonDocument> RawCollection
        {
            get { return Database.GetCollection<BsonDocument>(CollectionName()); }
        }

        public IMongoCollection<MongoEventCommit> TypedCollection
        {
            get { return Collection; }
        }

        public bool CanUseChangeStreams { get; private set; }

        public MongoEventStore(IMongoDatabase database, IEventNotifier notifier)
            : base(database)
        {
            Guard.NotNull(notifier, nameof(notifier));

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

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoEventCommit> collection, CancellationToken ct = default)
        {
            await collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoEventCommit>(
                    Index
                        .Ascending(x => x.Timestamp)),
                new CreateIndexModel<MongoEventCommit>(
                    Index
                        .Ascending(x => x.EventStream)
                        .Ascending(x => x.Timestamp)),
                new CreateIndexModel<MongoEventCommit>(
                    Index
                        .Ascending(x => x.EventStream)
                        .Descending(x => x.Timestamp)),
                new CreateIndexModel<MongoEventCommit>(
                    Index
                        .Ascending(x => x.EventStream)
                        .Descending(x => x.EventStreamOffset),
                    new CreateIndexOptions
                    {
                        Unique = true
                    })
            }, ct);

            var clusterVersion = await Database.GetVersionAsync();
            var clustered = Database.Client.Cluster.Description.Type == ClusterType.ReplicaSet;

            CanUseChangeStreams = clustered && clusterVersion >= new Version("4.0");
        }
    }
}