// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Squidex.MongoDb.TestHelpers;

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
