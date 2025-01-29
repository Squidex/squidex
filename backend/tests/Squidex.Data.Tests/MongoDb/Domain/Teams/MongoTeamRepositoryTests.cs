// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Teams;

[Trait("Category", "TestContainer")]
public class MongoTeamRepositoryTests(MongoFixture fixture) : TeamRepositoryTests, IClassFixture<MongoFixture>
{
    protected override async Task<ITeamRepository> CreateSutAsync()
    {
        var sut = new MongoTeamRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
