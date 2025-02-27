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
    public async Task UpdateAsync(
        CancellationToken ct)
    {
        var batchSize = rebuildOptions.Value.CalculateBatchSize();

        await rebuilder.RebuildAppsAsync(batchSize, ct);
        await rebuilder.RebuildSchemasAsync(batchSize, ct);
        await rebuilder.RebuildRulesAsync(batchSize, ct);
        await rebuilder.RebuildContentAsync(batchSize, ct);
        await rebuilder.RebuildAssetsAsync(batchSize, ct);
        await rebuilder.RebuildAssetFoldersAsync(batchSize, ct);
        await rebuilder.RebuildTeamsAsync(batchSize, ct);
    }
}
