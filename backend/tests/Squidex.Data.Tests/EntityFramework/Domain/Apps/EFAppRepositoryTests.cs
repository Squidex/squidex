// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Apps;

public abstract class EFAppRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : AppRepositoryTests where TContext : DbContext
{
    protected override Task<IAppRepository> CreateSutAsync()
    {
        var sut = new EFAppRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IAppRepository>(sut);
    }
}
