// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex;

public abstract class ContentDbContext(string prefix, IJsonSerializer jsonSerializer) : DbContext, IDbContextWithDialect
{
    public string Prefix { get; } = prefix;

    public abstract SqlDialect Dialect { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseContent(jsonSerializer, Dialect.JsonColumnType(), Prefix);

        base.OnModelCreating(modelBuilder);
    }
}
