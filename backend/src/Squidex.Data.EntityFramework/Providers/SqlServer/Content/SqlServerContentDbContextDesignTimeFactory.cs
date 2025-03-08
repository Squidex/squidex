// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;
using Squidex.Infrastructure.Json.System;

namespace Squidex.Providers.SqlServer.Content;

public sealed class SqlServerContentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerContentDbContext>
{
    public SqlServerContentDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=14330;Database=test;User=sa;Password=sqlserver";

        return new SqlServerContentDbContext(string.Empty, ConnectionString, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
