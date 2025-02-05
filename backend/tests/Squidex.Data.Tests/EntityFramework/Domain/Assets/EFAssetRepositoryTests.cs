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
[Collection("Postgres")]
public class EFAssetRepositoryTests(PostgresFixture fixture) : AssetRepositoryTests
{
    protected override Task<IAssetRepository> CreateSutAsync()
    {
        var sut = new EFAssetRepository<TestDbContextPostgres>(fixture.DbContextFactory, fixture.Dialect);

        return Task.FromResult<IAssetRepository>(sut);
    }
}
