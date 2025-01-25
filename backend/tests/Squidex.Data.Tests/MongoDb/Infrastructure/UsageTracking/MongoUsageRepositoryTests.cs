// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.UsageTracking;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.UsageTracking;

public class MongoUsageRepositoryTests(MongoFixture fixture) : UsageRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoUsageRepository sut = new MongoUsageRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IUsageRepository> CreateSutAsync()
    {
        return Task.FromResult<IUsageRepository>(sut);
    }
}