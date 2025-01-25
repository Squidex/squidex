// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.UsageTracking;

public class EFUsageRepositoryTests(PostgresFixture fixture) : UsageRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IUsageRepository> CreateSutAsync()
    {
        var sut = new EFUsageRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<IUsageRepository>(sut);
    }
}