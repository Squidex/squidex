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

namespace Squidex.Providers.MySql.App;

public sealed class MySqlAppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MySqlAppDbContext>
{
    public MySqlAppDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql";

        var builder = new DbContextOptionsBuilder<MySqlAppDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString), options =>
            {
                options.UseNetTopologySuite();
            });

        return new MySqlAppDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
