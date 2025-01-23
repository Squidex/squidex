// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.Postgres;

namespace Squidex.EntityFramework.Infrastructure.Queries;

public class PostgresQueryTests(PostgresFixture fixture) : SqlQueryTests, IClassFixture<PostgresFixture>
{
    protected override async Task<TestDbContext> CreateDbContextAsync()
    {
        var context = await fixture.DbContextFactory.CreateDbContextAsync();

        return context;
    }

    protected override SqlDialect CreateDialect()
    {
        return PostgresDialect.Instance;
    }
}
