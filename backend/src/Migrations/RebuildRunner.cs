// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Migrations.Migrations;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Migrations
{
    public sealed class RebuildRunner
    {
        private readonly RebuildFiles rebuildFiles;
        private readonly IAssetRepository assetRepository;
        private readonly Rebuilder rebuilder;
        private readonly PopulateGrainIndexes populateGrainIndexes;
        private readonly IContentRepository contentRepository;
        private readonly RebuildOptions rebuildOptions;

        public RebuildRunner(
            IOptions<RebuildOptions> rebuildOptions,
            IAssetRepository assetRepository,
            Rebuilder rebuilder,
            RebuildFiles rebuildFiles,
            PopulateGrainIndexes populateGrainIndexes,
            IContentRepository contentRepository)
        {
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(rebuildFiles, nameof(rebuildFiles));
            Guard.NotNull(rebuilder, nameof(rebuilder));
            Guard.NotNull(rebuildOptions, nameof(rebuildOptions));
            Guard.NotNull(populateGrainIndexes, nameof(populateGrainIndexes));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.assetRepository = assetRepository;
            this.rebuildFiles = rebuildFiles;
            this.rebuilder = rebuilder;
            this.rebuildOptions = rebuildOptions.Value;
            this.populateGrainIndexes = populateGrainIndexes;
            this.contentRepository = contentRepository;
        }

        public async Task RunAsync(CancellationToken ct)
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

            if (rebuildOptions.AssetsCount)
            {
                await assetRepository.RebuildCountsAsync(ct);
            }

            if (rebuildOptions.AssetFiles)
            {
                await rebuildFiles.RepairAsync(ct);
            }

            if (rebuildOptions.Contents)
            {
                await rebuilder.RebuildContentAsync(batchSize, ct);
            }

            if (rebuildOptions.ContentsCount)
            {
                await contentRepository.RebuildCountsAsync(ct);
            }

            if (rebuildOptions.Indexes)
            {
                await populateGrainIndexes.UpdateAsync(ct);
            }
        }
    }
}
