// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public class Migration04_FlattenAssetEntity : IMigration
    {
        private readonly Rebuilder rebuilder;

        public int FromVersion { get; } = 3;

        public int ToVersion { get; } = 4;

        public Migration04_FlattenAssetEntity(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public async Task UpdateAsync(IEnumerable<IMigration> previousMigrations)
        {
            if (!previousMigrations.Any(x => x is Migration01_FromCqrs))
            {
                await rebuilder.RebuildAssetsAsync();
            }
        }
    }
}
