// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.MongoDb.TestHelpers;
using Testcontainers.MongoDb;

namespace Squidex.MongoDb.Domain.Contents.Text;

public sealed class MongoTextIndexFixture : IAsyncLifetime
{
    private readonly MongoDbContainer mongoDb =
        new MongoDbBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "mongo-text-index")
            .Build();

    public MongoTextIndex Index { get; private set; }

    public MongoTextIndexFixture()
    {
        MongoTestUtils.SetupBson();
    }

    public async Task InitializeAsync()
    {
        await mongoDb.StartAsync();

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]!);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]!);

        Index = new MongoTextIndex(mongoDatabase, string.Empty);

        await Index.InitializeAsync(default);
    }

    public async Task DisposeAsync()
    {
        await mongoDb.StopAsync();
    }
}
