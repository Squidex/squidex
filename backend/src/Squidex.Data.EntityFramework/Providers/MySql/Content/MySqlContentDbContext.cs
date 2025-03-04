// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace Squidex.Providers.MySql.Content;

public sealed class MySqlContentDbContext(string prefix, string connectionString, IJsonSerializer jsonSerializer)
    : ContentDbContext(prefix, jsonSerializer)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.SetDefaultWarnings();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
        {
            options.UseNetTopologySuite();
        });

        base.OnConfiguring(optionsBuilder);
    }

    protected override string? JsonColumnType()
    {
        return "jsonb";
    }
}
