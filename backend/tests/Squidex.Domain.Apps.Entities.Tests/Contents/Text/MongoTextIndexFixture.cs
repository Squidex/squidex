// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class MongoTextIndexFixture : IAsyncLifetime
{
    public MongoTextIndex Index { get; }

    public MongoTextIndexFixture()
    {
        TestUtils.SetupBson();

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]!);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]!);

        Index = new MongoTextIndex(mongoDatabase, string.Empty);
    }

    public Task InitializeAsync()
    {
        return Index.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
