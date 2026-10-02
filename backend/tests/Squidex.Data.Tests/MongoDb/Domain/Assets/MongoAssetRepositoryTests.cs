// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Assets;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoAssetRepositoryTests : AssetRepositoryTests, IAsyncLifetime
{
    private readonly IMongoDatabase database;
    private readonly MongoQueryProfiler profiler;

    public MongoAssetRepositoryTests(MongoFixture fixture)
    {
        // The profiler works per database, therefore we use a dedicated one to not record queries from other tests.
        database = fixture.Client.GetDatabase("Test_Assets");

        profiler = new MongoQueryProfiler(database, "States_Assets2");
    }

    public async ValueTask InitializeAsync()
    {
        await profiler.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await profiler.StopAsync();
        await profiler.AssertIndexedAsync();
    }

    protected override async Task<IAssetRepository> CreateSutAsync()
    {
        var sut = new MongoAssetRepository(database, A.Fake<ILogger<MongoAssetRepository>>(), string.Empty);

        await sut.InitializeAsync(default);
        return sut;
    }
}
