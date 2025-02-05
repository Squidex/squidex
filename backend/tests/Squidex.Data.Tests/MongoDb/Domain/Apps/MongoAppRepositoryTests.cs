// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Apps;

[Trait("Category", "TestContainer")]
[Collection("Mongo")]
public class MongoAppRepositoryTests(MongoFixture fixture) : AppRepositoryTests
{
    protected override async Task<IAppRepository> CreateSutAsync()
    {
        var sut = new MongoAppRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
