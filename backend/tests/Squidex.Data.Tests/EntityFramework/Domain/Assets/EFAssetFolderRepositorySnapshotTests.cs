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
public class EFAssetFolderRepositorySnapshotTests(PostgresFixture fixture) : AssetFolderSnapshotStoreTests, IClassFixture<PostgresFixture>
{
    protected override Task<ISnapshotStore<AssetFolder>> CreateSutAsync()
    {
        var sut = new EFAssetFolderRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<AssetFolder>>(sut);
    }
}
