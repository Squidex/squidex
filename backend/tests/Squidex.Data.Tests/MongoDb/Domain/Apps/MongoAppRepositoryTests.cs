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
public class MongoAppRepositoryTests(MongoFixture fixture) : AppRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoAppRepository sut = new MongoAppRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IAppRepository> CreateSutAsync()
    {
        return Task.FromResult<IAppRepository>(sut);
    }
}
