// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations;

public sealed class RebuildSchemas : IMigration
{
    private readonly Rebuilder rebuilder;
    private readonly RebuildOptions rebuildOptions;

    public RebuildSchemas(Rebuilder rebuilder,
        IOptions<RebuildOptions> rebuildOptions)
    {
        this.rebuilder = rebuilder;
        this.rebuildOptions = rebuildOptions.Value;
    }

    public Task UpdateAsync(
        CancellationToken ct)
    {
        return rebuilder.RebuildSchemasAsync(rebuildOptions.BatchSize, ct);
    }
}
