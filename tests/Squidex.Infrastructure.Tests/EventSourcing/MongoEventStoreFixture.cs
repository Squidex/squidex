// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class MongoEventStoreFixture : IDisposable
    {
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost");
        private readonly IMongoDatabase mongoDatabase;
        private readonly IEventNotifier notifier = A.Fake<IEventNotifier>();

        public MongoEventStore EventStore { get; }

        public MongoEventStoreFixture()
        {
            mongoDatabase = mongoClient.GetDatabase("EventStoreTest");

            BsonJsonConvention.Register(JsonSerializer.Create(JsonHelper.DefaultSettings()));

            EventStore = new MongoEventStore(mongoDatabase, Options.Create(new MongoDbOptions()), notifier);
            EventStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            mongoClient.DropDatabase("EventStoreTest");
        }
    }
}
