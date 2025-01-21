// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.EntityFramework.TestHelpers;

namespace Squidex.EntityFramework.Domain.Teams;

public class EFTeamRepositoryTests(PostgresFixture fixture) : Shared.TeamRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<ITeamRepository> CreateSutAsync()
    {
        var sut = new EFTeamRepository<TestContext>(fixture.DbContextFactory);

        return Task.FromResult<ITeamRepository>(sut);
    }
}
