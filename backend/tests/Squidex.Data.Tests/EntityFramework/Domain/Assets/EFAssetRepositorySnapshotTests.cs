// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

[Trait("Category", "TestContainer")]
[Collection("Postgres")]
public class EFAssetRepositorySnapshotTests(PostgresFixture fixture) : AssetSnapshotStoreTests
{
    protected override Task<ISnapshotStore<Asset>> CreateSutAsync()
    {
        var sut = new EFAssetRepository<TestDbContextPostgres>(fixture.DbContextFactory, fixture.Dialect);

        return Task.FromResult<ISnapshotStore<Asset>>(sut);
    }
}
