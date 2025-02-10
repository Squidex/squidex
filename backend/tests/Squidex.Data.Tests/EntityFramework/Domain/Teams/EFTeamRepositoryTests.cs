// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Teams;

public abstract class EFTeamRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : TeamRepositoryTests where TContext : DbContext
{
    protected override Task<ITeamRepository> CreateSutAsync()
    {
        var sut = new EFTeamRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ITeamRepository>(sut);
    }
}
