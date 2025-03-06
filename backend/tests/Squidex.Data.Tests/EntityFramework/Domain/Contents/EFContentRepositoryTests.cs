// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

public abstract class EFContentRepositoryTests<TContext, TContentContext>(ISqlContentFixture<TContext, TContentContext> fixture)
    : ContentRepositoryTests
    where TContext : DbContext, IDbContextWithDialect where TContentContext : ContentDbContext
{
    protected override Task<IContentRepository> CreateSutAsync()
    {
        var sut =
            new EFContentRepository<TContext, TContentContext>(
                fixture.DbContextFactory,
                fixture.DbContextNamedFactory,
                AppProvider,
                Options.Create(new ContentsOptions()));

        return Task.FromResult<IContentRepository>(sut);
    }
}
