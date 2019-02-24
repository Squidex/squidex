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
        public MongoEventStore EventStore { get; }

        public IMongoClient MongoClient { get; } = new MongoClient("mongodb://localhost");

        public IMongoDatabase MongoDatabase { get; }

        public IEventNotifier Notifier { get; } = A.Fake<IEventNotifier>();

        public MongoEventStoreFixture()
        {
            MongoDatabase = MongoClient.GetDatabase("EventStoreTest");

            BsonJsonConvention.Register(JsonSerializer.Create(JsonHelper.DefaultSettings()));

            EventStore = new MongoEventStore(MongoDatabase, Notifier);
            EventStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            MongoClient.DropDatabase("EventStoreTest");
        }
    }
}
