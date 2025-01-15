// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squidex.Events.Mongo;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.MongoDb.Infrastructure;

public sealed class MongoEventStoreFixture_Direct : MongoEventStoreFixture
{
    public MongoEventStoreFixture_Direct()
        : base(TestConfig.Configuration["mongoDb:configurationDirect"]!)
    {
    }
}

public sealed class MongoEventStoreFixture_Replica : MongoEventStoreFixture
{
    public MongoEventStoreFixture_Replica()
        : base(TestConfig.Configuration["mongoDb:configurationReplica"]!)
    {
    }
}

public abstract class MongoEventStoreFixture : IAsyncLifetime
{
    public MongoEventStore EventStore { get; }

    public IMongoDatabase Database { get; }

    static MongoEventStoreFixture()
    {
        BsonJsonConvention.Register(TestUtils.DefaultOptions());
    }

    protected MongoEventStoreFixture(string connectionString)
    {
        var mongoClient = MongoClientFactory.Create(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        Database = mongoDatabase;

        EventStore = new MongoEventStore(mongoDatabase, Options.Create(new MongoEventStoreOptions()));
    }

    public Task InitializeAsync()
    {
        return EventStore.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
