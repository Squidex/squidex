// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Commands;

namespace Migrations;

public sealed class RebuildRunner(
    IOptions<RebuildOptions> rebuildOptions,
    Rebuilder rebuilder,
    RebuildFiles rebuildFiles)
{
    public async Task RunAsync(
        CancellationToken ct)
    {
        var batchSize = rebuildOptions.Value.CalculateBatchSize();

        if (rebuildOptions.Value.Apps)
        {
            await rebuilder.RebuildAppsAsync(batchSize, ct);
        }

        if (rebuildOptions.Value.Teams)
        {
            await rebuilder.RebuildTeamsAsync(batchSize, ct);
        }

        if (rebuildOptions.Value.Schemas)
        {
            await rebuilder.RebuildSchemasAsync(batchSize, ct);
        }

        if (rebuildOptions.Value.Rules)
        {
            await rebuilder.RebuildRulesAsync(batchSize, ct);
        }

        if (rebuildOptions.Value.Assets)
        {
            await rebuilder.RebuildAssetsAsync(batchSize, ct);
            await rebuilder.RebuildAssetFoldersAsync(batchSize, ct);
        }

        if (rebuildOptions.Value.AssetFiles)
        {
            await rebuildFiles.RepairAsync(ct);
        }

        if (rebuildOptions.Value.Contents)
        {
            await rebuilder.RebuildContentAsync(batchSize, ct);
        }
    }
}
