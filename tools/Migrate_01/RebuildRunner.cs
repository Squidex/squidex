// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Migrate_01.Migrations;
using Squidex.Infrastructure;

namespace Migrate_01
{
    public sealed class RebuildRunner
    {
        private readonly Rebuilder rebuilder;
        private readonly BuildFullTextIndices fullTextIndices;
        private readonly RebuildOptions rebuildOptions;

        public RebuildRunner(Rebuilder rebuilder, BuildFullTextIndices fullTextIndices, IOptions<RebuildOptions> rebuildOptions)
        {
            Guard.NotNull(rebuilder, nameof(rebuilder));
            Guard.NotNull(rebuildOptions, nameof(rebuildOptions));
            Guard.NotNull(fullTextIndices, nameof(fullTextIndices));

            this.rebuilder = rebuilder;
            this.rebuildOptions = rebuildOptions.Value;
            this.fullTextIndices = fullTextIndices;
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
            }

            if (rebuildOptions.Contents)
            {
                await rebuilder.RebuildContentAsync(ct);
            }

            if (rebuildOptions.Indices)
            {
                await fullTextIndices.UpdateAsync(ct);
            }
        }
    }
}
