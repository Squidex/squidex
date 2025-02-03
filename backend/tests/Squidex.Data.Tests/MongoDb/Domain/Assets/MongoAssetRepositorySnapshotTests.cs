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
[Collection("Mongo")]
public class MongoAssetRepositorySnapshotTests(MongoFixture fixture) : AssetSnapshotStoreTests
{
    protected override async Task<ISnapshotStore<Asset>> CreateSutAsync()
    {
        var sut = new MongoAssetRepository(fixture.Database, A.Fake<ILogger<MongoAssetRepository>>(), string.Empty);

        await sut.InitializeAsync(default);
        return sut;
    }
}
