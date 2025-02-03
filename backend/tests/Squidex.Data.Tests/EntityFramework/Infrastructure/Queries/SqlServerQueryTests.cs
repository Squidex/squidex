﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.SqlServer;

namespace Squidex.EntityFramework.Infrastructure.Queries;

[Trait("Category", "TestContainer")]
[Collection("SqlServer")]
public class SqlServerQueryTests(SqlServerFixture fixture) : SqlQueryTests<TestDbContext>
{
    protected override async Task<TestDbContext> CreateDbContextAsync()
    {
        var context = await fixture.DbContextFactory.CreateDbContextAsync();

        return context;
    }

    protected override SqlDialect CreateDialect()
    {
        return SqlServerDialect.Instance;
    }
}
