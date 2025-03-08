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

namespace Squidex.Providers.Postgres.App;

public sealed class PostgresAppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=54320;Database=test;User=postgres;Password=postgres";

        var builder = new DbContextOptionsBuilder<PostgresAppDbContext>()
            .UseNpgsql(ConnectionString, options =>
            {
                options.UseNetTopologySuite();
            });

        return new PostgresAppDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
