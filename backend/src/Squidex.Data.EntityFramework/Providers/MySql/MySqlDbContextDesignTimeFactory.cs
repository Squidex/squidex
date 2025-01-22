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

namespace Squidex.Providers.MySql;

public sealed class MySqlDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    public MySqlDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql";

        var builder = new DbContextOptionsBuilder<MySqlDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));

        return new MySqlDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
