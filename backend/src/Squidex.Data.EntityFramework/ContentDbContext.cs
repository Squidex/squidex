// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex;

public abstract class ContentDbContext(string prefix, IJsonSerializer jsonSerializer) : DbContext
{
    public string Prefix { get; } = prefix;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseContent(jsonSerializer, JsonColumnType(), Prefix);

        base.OnModelCreating(modelBuilder);
    }

    public async Task MigrateAsync(
        CancellationToken ct)
    {
        TableName.Prefix = Prefix;

        await Database.MigrateAsync(ct);
    }

    protected virtual string? JsonColumnType()
    {
        return null;
    }
}
