// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.TestHelpers;

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
            mongoDatabase = mongoClient.GetDatabase("Squidex_Testing");

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
            : base("mongodb://localhost:27019")
        {
        }
    }

    public sealed class MongoEventStoreReplicaSetFixture : MongoEventStoreFixture
    {
        public MongoEventStoreReplicaSetFixture()
            : base("mongodb://localhost:27017")
        {
        }
    }
}
