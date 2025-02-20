// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Squidex.Infrastructure.Json.System;

namespace Squidex.Providers.SqlServer;

public sealed class SqlServerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=14330;Database=test;User=sa;Password=sqlserver";

        var builder = new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer(ConnectionString, options =>
            {
                options.UseNetTopologySuite();
            });

        return new SqlServerDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}