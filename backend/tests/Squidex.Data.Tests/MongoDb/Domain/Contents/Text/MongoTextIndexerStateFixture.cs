// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Entities.Text;
using Squidex.Infrastructure;
using Squidex.TestHelpers;
using Testcontainers.MongoDb;

namespace Squidex.MongoDb.Domain.Contents.Text;

public sealed class MongoTextIndexerStateFixture : IAsyncLifetime
{
    private readonly MongoDbContainer mongoDb =
        new MongoDbBuilder()
            .WithReuse(true)
            .WithLabel("reuse-id", "mongo-text-indexer")
            .Build();

    public IContentRepository ContentRepository { get; } = A.Fake<IContentRepository>();

    public MongoTextIndexerState State { get; private set; }

    public MongoTextIndexerStateFixture()
    {
        MongoTestUtils.SetupBson();
    }

    public async Task InitializeAsync()
    {
        await mongoDb.StartAsync();

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]!);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]!);

        State = new MongoTextIndexerState(mongoDatabase, ContentRepository);

        await State.InitializeAsync(default);
    }

    public async Task DisposeAsync()
    {
        await mongoDb.DisposeAsync();
    }
}
