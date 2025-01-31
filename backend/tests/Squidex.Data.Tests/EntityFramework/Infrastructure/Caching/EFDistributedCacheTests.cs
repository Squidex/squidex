// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Caching;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Caching;

[Trait("Category", "TestContainer")]
public class EFDistributedCacheTests(PostgresFixture fixture) : DistributedCacheTests, IClassFixture<PostgresFixture>
{
    protected override Task<IDistributedCache> CreateSutAsync(TimeProvider timeProvider)
    {
        var sut = new EFDistributedCache<TestDbContextPostgres>(fixture.DbContextFactory, timeProvider);

        return Task.FromResult<IDistributedCache>(sut);
    }
}
