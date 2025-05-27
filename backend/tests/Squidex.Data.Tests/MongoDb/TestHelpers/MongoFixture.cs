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
        new MongoDbBuilder()
            .WithReuse(false)
            .WithLabel("reuse-id", "squidex-mongodb")
            .Build();

    public IMongoClient Client { get; private set; }

    public IMongoDatabase Database => Client.GetDatabase("Test");

    public MongoFixture()
    {
        MongoTestUtils.SetupBson();
    }

    public async Task InitializeAsync()
    {
        await mongoDb.StartAsync();

        Client = new MongoClient(mongoDb.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await mongoDb.StopAsync();
    }
}
