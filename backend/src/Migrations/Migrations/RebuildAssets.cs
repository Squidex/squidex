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

public sealed class RebuildAssets(Rebuilder rebuilder,
    IOptions<RebuildOptions> rebuildOptions)
    : IMigration
{
    private readonly RebuildOptions rebuildOptions = rebuildOptions.Value;

    public Task UpdateAsync(
        CancellationToken ct)
    {
        return rebuilder.RebuildAssetsAsync(rebuildOptions.BatchSize, ct);
    }
}
