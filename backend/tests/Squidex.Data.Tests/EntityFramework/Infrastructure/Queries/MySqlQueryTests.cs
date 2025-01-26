// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Queries;
using Squidex.Providers.MySql;

namespace Squidex.EntityFramework.Infrastructure.Queries;

[Trait("Category", "TestContainer")]
public class MySqlQueryTests(MySqlFixture fixture) : SqlQueryTests<TestDbContextMySql>, IClassFixture<MySqlFixture>
{
    protected override async Task<TestDbContextMySql> CreateDbContextAsync()
    {
        var context = await fixture.DbContextFactory.CreateDbContextAsync();

        return context;
    }

    protected override SqlDialect CreateDialect()
    {
        return MySqlDialect.Instance;
    }
}
