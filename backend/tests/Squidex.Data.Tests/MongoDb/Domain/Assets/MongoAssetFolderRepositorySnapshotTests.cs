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

namespace Squidex.MongoDb.Domain.Assets;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoAssetFolderRepositorySnapshotTests(MongoFixture fixture) : AssetFolderSnapshotStoreTests
{
    protected override async Task<ISnapshotStore<AssetFolder>> CreateSutAsync()
    {
        var sut = new MongoAssetFolderRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
