// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Infrastructure.States;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Assets;

[Trait("Category", "TestContainer")]
public class MongoAssetRepositorySnapshotTests(MongoFixture fixture) : AssetSnapshotStoreTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoAssetRepository sut = new MongoAssetRepository(fixture.Database, A.Fake<ILogger<MongoAssetRepository>>(), string.Empty);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<ISnapshotStore<Asset>> CreateSutAsync()
    {
        return Task.FromResult<ISnapshotStore<Asset>>(sut);
    }
}
