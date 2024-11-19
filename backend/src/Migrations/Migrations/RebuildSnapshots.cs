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

public sealed class RebuildSnapshots(
    Rebuilder rebuilder,
    IOptions<RebuildOptions> rebuildOptions)
    : IMigration
{
    private readonly RebuildOptions rebuildOptions = rebuildOptions.Value;

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        await rebuilder.RebuildAppsAsync(rebuildOptions.BatchSize, ct);
        await rebuilder.RebuildSchemasAsync(rebuildOptions.BatchSize, ct);
        await rebuilder.RebuildRulesAsync(rebuildOptions.BatchSize, ct);
        await rebuilder.RebuildContentAsync(rebuildOptions.BatchSize, ct);
        await rebuilder.RebuildAssetsAsync(rebuildOptions.BatchSize, ct);
        await rebuilder.RebuildAssetFoldersAsync(rebuildOptions.BatchSize, ct);
    }
}
