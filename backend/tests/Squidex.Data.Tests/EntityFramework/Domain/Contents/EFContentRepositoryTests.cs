// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

public abstract class EFContentRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : ContentRepositoryTests where TContext : DbContext
{
    protected override Task<IContentRepository> CreateSutAsync()
    {
        var sut = new EFContentRepository<TContext>(fixture.DbContextFactory, AppProvider, fixture.Dialect);

        return Task.FromResult<IContentRepository>(sut);
    }
}
