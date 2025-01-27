// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

[Trait("Category", "TestContainer")]
public class EFAssetFolderRepositoryTests(PostgresFixture fixture) : AssetFolderRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IAssetFolderRepository> CreateSutAsync()
    {
        var sut = new EFAssetFolderRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<IAssetFolderRepository>(sut);
    }
}
