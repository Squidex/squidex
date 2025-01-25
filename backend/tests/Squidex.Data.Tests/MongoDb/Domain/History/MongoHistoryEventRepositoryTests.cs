// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.History;

public class MongoHistoryEventRepositoryTests(MongoFixture fixture) : HistoryEventRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoHistoryEventRepository sut = new MongoHistoryEventRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IHistoryEventRepository> CreateSutAsync()
    {
        return Task.FromResult<IHistoryEventRepository>(sut);
    }
}
