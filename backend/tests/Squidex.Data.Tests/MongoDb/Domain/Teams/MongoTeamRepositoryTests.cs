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

public class MongoTeamRepositoryTests(MongoFixture fixture) : TeamRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoTeamRepository sut = new MongoTeamRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<ITeamRepository> CreateSutAsync()
    {
        return Task.FromResult<ITeamRepository>(sut);
    }
}
