﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Teams;

[Trait("Category", "TestContainer")]
public class EFTeamRepositoryTests(PostgresFixture fixture) : TeamRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<ITeamRepository> CreateSutAsync()
    {
        var sut = new EFTeamRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<ITeamRepository>(sut);
    }
}
