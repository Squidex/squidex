// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.History;

public abstract class EFHistoryEventRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : HistoryEventRepositoryTests where TContext : DbContext
{
    protected override Task<IHistoryEventRepository> CreateSutAsync()
    {
        var sut = new EFHistoryEventRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IHistoryEventRepository>(sut);
    }
}
