﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Json;

namespace Squidex.Providers.Postgres;

public class PostgresDbContext(DbContextOptions options, IJsonSerializer jsonSerializer)
    : AppDbContext(options, jsonSerializer)
{
    protected override string? JsonColumnType()
    {
        return "jsonb";
    }
}
