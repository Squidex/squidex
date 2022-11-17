// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class AtlasTextIndexFixture : IAsyncLifetime
{
    public AtlasTextIndex Index { get; }

    public AtlasTextIndexFixture()
    {
        TestUtils.SetupBson();

        var mongoClient = new MongoClient(TestConfig.Configuration["atlas:configuration"]);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["atlas:database"]);

        var options = TestConfig.Configuration.GetSection("atlas").Get<AtlasOptions>()!;

        Index = new AtlasTextIndex(mongoDatabase, Options.Create(options));
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
