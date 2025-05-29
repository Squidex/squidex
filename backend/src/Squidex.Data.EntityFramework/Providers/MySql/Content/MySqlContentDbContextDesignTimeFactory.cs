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
using Squidex.Providers.MySql.App;

namespace Squidex.Providers.MySql.Content;

public sealed class MySqlContentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MySqlContentDbContext>
{
    public MySqlContentDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql";

        var builder = new DbContextOptionsBuilder<MySqlAppDbContext>()
            .UsePrefix(string.Empty)
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString), options =>
            {
                options.UseNetTopologySuite();
            });

        return new MySqlContentDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
