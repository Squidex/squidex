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
    private readonly RebuildOptions rebuildOptions = rebuildOptions.Value;

    public async Task RunAsync(
        CancellationToken ct)
    {
        var batchSize = rebuildOptions.CalculateBatchSize();

        if (rebuildOptions.Apps)
        {
            await rebuilder.RebuildAppsAsync(batchSize, ct);
        }

        if (rebuildOptions.Schemas)
        {
            await rebuilder.RebuildSchemasAsync(batchSize, ct);
        }

        if (rebuildOptions.Rules)
        {
            await rebuilder.RebuildRulesAsync(batchSize, ct);
        }

        if (rebuildOptions.Assets)
        {
            await rebuilder.RebuildAssetsAsync(batchSize, ct);
            await rebuilder.RebuildAssetFoldersAsync(batchSize, ct);
        }

        if (rebuildOptions.AssetFiles)
        {
            await rebuildFiles.RepairAsync(ct);
        }

        if (rebuildOptions.Contents)
        {
            await rebuilder.RebuildContentAsync(batchSize, ct);
        }
    }
}
