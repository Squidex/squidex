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
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Assets;

public abstract class EFAssetFolderRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : AssetFolderRepositoryTests where TContext : DbContext
{
    protected override Task<IAssetFolderRepository> CreateSutAsync()
    {
        var sut = new EFAssetFolderRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IAssetFolderRepository>(sut);
    }
}
