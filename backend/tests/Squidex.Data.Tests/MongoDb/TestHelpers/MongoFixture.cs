// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Testcontainers.MongoDb;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.MongoDb.TestHelpers;

[CollectionDefinition(Name)]
public sealed class MongoFixtureCollection : ICollectionFixture<MongoFixture>
{
    public const string Name = "Mongo";
}

public class MongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer mongoDb =
        new MongoDbBuilder("mongo:6.0")
            .WithReuse(false)
            .WithLabel("reuse-id", "squidex-mongodb")
            .Build();

    public IMongoClient Client { get; private set; }

    public IMongoDatabase Database => Client.GetDatabase("Test");

    public async ValueTask InitializeAsync()
    {
        await mongoDb.StartAsync(TestContext.Current.CancellationToken);

        Client = MongoClientFactory.Create(mongoDb.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        await mongoDb.StopAsync(TestContext.Current.CancellationToken);
    }
}
