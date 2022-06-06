// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.TestHelpers;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing
{
    public abstract class MongoEventStoreFixture : IDisposable
    {
        private readonly IMongoClient mongoClient;
        private readonly IMongoDatabase mongoDatabase;
        private readonly IEventNotifier notifier = A.Fake<IEventNotifier>();

        public MongoEventStore EventStore { get; }

        protected MongoEventStoreFixture(string connectionString)
        {
            mongoClient = new MongoClient(connectionString);
            mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

            BsonJsonConvention.Register(JsonSerializer.Create(TestUtils.DefaultSettings()));

            EventStore = new MongoEventStore(mongoDatabase, notifier);
            EventStore.InitializeAsync(default).Wait();
        }

        public void Cleanup()
        {
            mongoClient.DropDatabase("EventStoreTest");
        }

        public void Dispose()
        {
            Cleanup();

            GC.SuppressFinalize(this);
        }
    }

    public sealed class MongoEventStoreDirectFixture : MongoEventStoreFixture
    {
        public MongoEventStoreDirectFixture()
            : base(TestConfig.Configuration["mongodb:configuration"])
        {
        }
    }

    public sealed class MongoEventStoreReplicaSetFixture : MongoEventStoreFixture
    {
        public MongoEventStoreReplicaSetFixture()
            : base(TestConfig.Configuration["mongodb:configurationReplica"])
        {
        }
    }
}
