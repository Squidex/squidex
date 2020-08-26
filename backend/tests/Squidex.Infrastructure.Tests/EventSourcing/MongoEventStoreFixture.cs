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
    public sealed class MongoEventStoreFixture : IDisposable
    {
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private readonly IMongoDatabase mongoDatabase;
        private readonly IEventNotifier notifier = A.Fake<IEventNotifier>();

        public MongoEventStore EventStore { get; }

        public MongoEventStoreFixture()
        {
            Dispose();

            mongoDatabase = mongoClient.GetDatabase($"EventStoreTest2");

            BsonJsonConvention.Register(JsonSerializer.Create(JsonHelper.DefaultSettings()));

            EventStore = new MongoEventStore(mongoDatabase, notifier);
            EventStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            mongoClient.DropDatabase("EventStoreTest2");
        }
    }
}
