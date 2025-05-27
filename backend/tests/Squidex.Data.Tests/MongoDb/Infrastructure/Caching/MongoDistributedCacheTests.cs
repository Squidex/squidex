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

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDistributedCacheTests(MongoFixture fixture) : DistributedCacheTests
{
    protected override async Task<IDistributedCache> CreateSutAsync(TimeProvider timeProvider)
    {
        var sut = new MongoDistributedCache(fixture.Database, timeProvider);

        await sut.InitializeAsync(default);
        return sut;
    }
}
