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
public class EFAssetRepositoryTests(PostgresFixture fixture) : AssetRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IAssetRepository> CreateSutAsync()
    {
        var sut = new EFAssetRepository<TestDbContext>(fixture.DbContextFactory, fixture.Dialect);

        return Task.FromResult<IAssetRepository>(sut);
    }
}
