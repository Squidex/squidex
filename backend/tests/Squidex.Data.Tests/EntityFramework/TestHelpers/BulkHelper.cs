// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EFCore.BulkExtensions.SqlAdapters;
using Squidex.Providers.Postgres.Content;

namespace Squidex.EntityFramework.TestHelpers;

public static class BulkHelper
{
    public static void Configure()
    {
        SqlAdaptersMapping.Provider = context =>
        {
            switch (context)
            {
                case PostgresContentDbContext:
                    return TestDbContextMySql.Server;
                case TestDbContextMySql:
                    return TestDbContextMySql.Server;
                case TestDbContextPostgres:
                    return TestDbContextPostgres.Server;
                case TestDbContextSqlServer:
                    return TestDbContextSqlServer.Server;
            }

            throw new ArgumentException("Not supported.", nameof(context));
        };
    }
}
