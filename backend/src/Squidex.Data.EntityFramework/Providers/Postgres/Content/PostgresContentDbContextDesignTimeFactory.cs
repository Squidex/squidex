// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;
using Squidex.Infrastructure.Json.System;

namespace Squidex.Providers.Postgres.Content;

public sealed class PostgresContentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PostgresContentDbContext>
{
    public PostgresContentDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=54320;Database=test;User=postgres;Password=postgres";

        return new PostgresContentDbContext(string.Empty, ConnectionString, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
