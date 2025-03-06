// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

public abstract class EFAssetRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : AssetRepositoryTests
    where TContext : DbContext, IDbContextWithDialect
{
    protected override Task<IAssetRepository> CreateSutAsync()
    {
        var sut = new EFAssetRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IAssetRepository>(sut);
    }
}
