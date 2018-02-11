// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
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
            await rebuilder.RebuildConfigAsync();
            await rebuilder.RebuildContentAsync();
            await rebuilder.RebuildAssetsAsync();
        }
    }
}
