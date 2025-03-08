// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EFCore.BulkExtensions.SqlAdapters;
using EFCore.BulkExtensions.SqlAdapters.MySql;
using EFCore.BulkExtensions.SqlAdapters.PostgreSql;
using EFCore.BulkExtensions.SqlAdapters.SqlServer;
using Squidex.Providers.MySql.Content;
using Squidex.Providers.Postgres.Content;
using Squidex.Providers.SqlServer.Content;

namespace Squidex.EntityFramework.TestHelpers;

public static class BulkHelper
{
    private static readonly IDbServer MySql = new MySqlDbServer();
    private static readonly IDbServer PostgreSql = new PostgreSqlDbServer();
    private static readonly IDbServer SqlServer = new SqlServerDbServer();

    public static void Configure()
    {
        SqlAdaptersMapping.Provider = context =>
        {
            switch (context)
            {
                case MySqlContentDbContext:
                    return MySql;
                case PostgresContentDbContext:
                    return PostgreSql;
                case SqlServerContentDbContext:
                    return SqlServer;
                case TestDbContextMySql:
                    return MySql;
                case TestDbContextPostgres:
                    return PostgreSql;
                case TestDbContextSqlServer:
                    return SqlServer;
            }

            throw new ArgumentException("Not supported.", nameof(context));
        };
    }
}
