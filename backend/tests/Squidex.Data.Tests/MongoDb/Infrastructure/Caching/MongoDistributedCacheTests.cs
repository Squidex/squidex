// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Squidex.Infrastructure.Caching;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.Caching;

public class MongoDistributedCacheTests(MongoFixture fixture) : DistributedCacheTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private MongoDistributedCache sut;

    public async Task InitializeAsync()
    {
        sut = new MongoDistributedCache(fixture.Database, TimeProvider);

        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IDistributedCache> CreateSutAsync()
    {
        return Task.FromResult<IDistributedCache>(sut);
    }
}
