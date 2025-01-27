// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Infrastructure.States;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.AssetFolders;

[Trait("Category", "TestContainer")]
public class MongoAssetFolderRepositorySnapshotTests(MongoFixture fixture) : AssetFolderSnapshotStoreTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoAssetFolderRepository sut = new MongoAssetFolderRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<ISnapshotStore<AssetFolder>> CreateSutAsync()
    {
        return Task.FromResult<ISnapshotStore<AssetFolder>>(sut);
    }
}
