// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.UsageTracking;

public abstract class EFUsageRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : UsageRepositoryTests where TContext : DbContext
{
    protected override Task<IUsageRepository> CreateSutAsync()
    {
        var sut = new EFUsageRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IUsageRepository>(sut);
    }
}
