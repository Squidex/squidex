// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Migrations.Migrations;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Migrations
{
    public sealed class RebuildRunner
    {
        private readonly RepairFiles repairFiles;
        private readonly Rebuilder rebuilder;
        private readonly PopulateGrainIndexes populateGrainIndexes;
        private readonly RebuildOptions rebuildOptions;

        public RebuildRunner(
            RepairFiles repairFiles,
            Rebuilder rebuilder,
            IOptions<RebuildOptions> rebuildOptions,
            PopulateGrainIndexes populateGrainIndexes)
        {
            Guard.NotNull(repairFiles, nameof(repairFiles));
            Guard.NotNull(rebuilder, nameof(rebuilder));
            Guard.NotNull(rebuildOptions, nameof(rebuildOptions));
            Guard.NotNull(populateGrainIndexes, nameof(populateGrainIndexes));

            this.repairFiles = repairFiles;
            this.rebuilder = rebuilder;
            this.rebuildOptions = rebuildOptions.Value;
            this.populateGrainIndexes = populateGrainIndexes;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            if (rebuildOptions.Apps)
            {
                await rebuilder.RebuildAppsAsync(ct);
            }

            if (rebuildOptions.Schemas)
            {
                await rebuilder.RebuildSchemasAsync(ct);
            }

            if (rebuildOptions.Rules)
            {
                await rebuilder.RebuildRulesAsync(ct);
            }

            if (rebuildOptions.Assets)
            {
                await rebuilder.RebuildAssetsAsync(ct);
                await rebuilder.RebuildAssetFoldersAsync(ct);
            }

            if (rebuildOptions.AssetFiles)
            {
                await repairFiles.RepairAsync(ct);
            }

            if (rebuildOptions.Contents)
            {
                await rebuilder.RebuildContentAsync(ct);
            }

            if (rebuildOptions.Indexes)
            {
                await populateGrainIndexes.UpdateAsync();
            }
        }
    }
}
