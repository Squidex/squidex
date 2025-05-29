// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.System;
using Squidex.Providers.SqlServer.App;

namespace Squidex.Providers.SqlServer.Content;

public sealed class SqlServerContentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerContentDbContext>
{
    public SqlServerContentDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=14330;Database=test;User=sa;Password=sqlserver";

        var builder = new DbContextOptionsBuilder<SqlServerAppDbContext>()
            .UsePrefix(string.Empty)
            .UseSqlServer(ConnectionString, options =>
            {
                options.UseNetTopologySuite();
            });

        return new SqlServerContentDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
