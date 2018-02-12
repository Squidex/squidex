// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public class RebuildAssets : IMigration
    {
        private readonly Rebuilder rebuilder;

        public RebuildAssets(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public Task UpdateAsync()
        {
            return rebuilder.RebuildAssetsAsync();
        }
    }
}
