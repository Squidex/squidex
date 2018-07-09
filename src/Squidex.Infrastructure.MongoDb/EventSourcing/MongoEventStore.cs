// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.EventSourcing
{
    public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        private static readonly FieldDefinition<MongoEventCommit, BsonTimestamp> TimestampField = Fields.Build(x => x.Timestamp);
        private static readonly FieldDefinition<MongoEventCommit, long> EventsCountField = Fields.Build(x => x.EventsCount);
        private static readonly FieldDefinition<MongoEventCommit, long> EventStreamOffsetField = Fields.Build(x => x.EventStreamOffset);
        private static readonly FieldDefinition<MongoEventCommit, string> EventStreamField = Fields.Build(x => x.EventStream);
        private readonly IEventNotifier notifier;

        public IMongoCollection<BsonDocument> RawCollection
        {
            get { return Database.GetCollection<BsonDocument>(CollectionName()); }
        }

        public MongoEventStore(IMongoDatabase database, IEventNotifier notifier)
            : base(database)
        {
            Guard.NotNull(notifier, nameof(notifier));

            this.notifier = notifier;
        }

        protected override string CollectionName()
        {
            return "Events";
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { ReadPreference = ReadPreference.Primary, WriteConcern = WriteConcern.WMajority };
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoEventCommit> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<MongoEventCommit>(Index.Ascending(x => x.Timestamp).Ascending(x => x.EventStream))),
                collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<MongoEventCommit>(Index.Ascending(x => x.EventStream).Descending(x => x.EventStreamOffset), new CreateIndexOptions { Unique = true })));
        }
    }
}