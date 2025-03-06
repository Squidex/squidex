// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;
using Squidex.Infrastructure.Json.System;

namespace Squidex.Providers.MySql.Content;

public sealed class MySqlContentDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MySqlContentDbContext>
{
    public MySqlContentDbContext CreateDbContext(string[] args)
    {
        const string ConnectionString = "Server=localhost;Port=33060;Database=test;User=mysql;Password=mysql";

        return new MySqlContentDbContext(string.Empty, ConnectionString, null, new SystemJsonSerializer(JsonSerializerOptions.Default));
    }
}
