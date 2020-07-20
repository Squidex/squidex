// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class RebuildSnapshots : IMigration
    {
        private readonly Rebuilder rebuilder;

        public RebuildSnapshots(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public async Task UpdateAsync()
        {
            await rebuilder.RebuildAppsAsync();
            await rebuilder.RebuildSchemasAsync();
            await rebuilder.RebuildRulesAsync();
            await rebuilder.RebuildContentAsync();
            await rebuilder.RebuildAssetsAsync();
            await rebuilder.RebuildAssetFoldersAsync();
        }
    }
}
