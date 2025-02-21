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

namespace Squidex.Providers.Postgres;

public sealed class PostgresDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
    public PostgresDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=54320;Database=test;User=postgres;Password=postgres";

        var builder = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(ConnectionString, options =>
            {
                options.UseNetTopologySuite();
            });

        return new PostgresDbContext(builder.Options, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
