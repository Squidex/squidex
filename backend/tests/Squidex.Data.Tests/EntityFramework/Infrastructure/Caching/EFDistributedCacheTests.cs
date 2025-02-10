// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Caching;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Caching;

public abstract class EFDistributedCacheTests<TContext>(ISqlFixture<TContext> fixture) : DistributedCacheTests where TContext : DbContext
{
    protected override Task<IDistributedCache> CreateSutAsync(TimeProvider timeProvider)
    {
        var sut = new EFDistributedCache<TContext>(fixture.DbContextFactory, timeProvider);

        return Task.FromResult<IDistributedCache>(sut);
    }
}
