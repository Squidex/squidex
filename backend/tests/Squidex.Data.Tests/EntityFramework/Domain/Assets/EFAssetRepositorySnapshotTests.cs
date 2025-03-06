// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

public abstract class EFAssetRepositorySnapshotTests<TContext>(ISqlFixture<TContext> fixture) : AssetSnapshotStoreTests
    where TContext : DbContext, IDbContextWithDialect
{
    protected override Task<ISnapshotStore<Asset>> CreateSutAsync()
    {
        var sut = new EFAssetRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<Asset>>(sut);
    }
}
