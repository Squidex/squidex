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
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

public abstract class EFAssetFolderRepositorySnapshotTests<TContext>(ISqlFixture<TContext> fixture) : AssetFolderSnapshotStoreTests where TContext : DbContext
{
    protected override Task<ISnapshotStore<AssetFolder>> CreateSutAsync()
    {
        var sut = new EFAssetFolderRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<AssetFolder>>(sut);
    }
}
