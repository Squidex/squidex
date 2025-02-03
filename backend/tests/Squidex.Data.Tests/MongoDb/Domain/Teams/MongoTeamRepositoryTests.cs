﻿// ==========================================================================
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
[Collection("Mongo")]
public class MongoTeamRepositoryTests(MongoFixture fixture) : TeamRepositoryTests
{
    protected override async Task<ITeamRepository> CreateSutAsync()
    {
        var sut = new MongoTeamRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
