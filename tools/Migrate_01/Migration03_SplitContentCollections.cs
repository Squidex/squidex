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
    public class Migration03_SplitContentCollections : IMigration
    {
        private readonly Rebuilder rebuilder;

        public int FromVersion { get; } = 2;

        public int ToVersion { get; } = 3;

        public Migration03_SplitContentCollections(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public async Task UpdateAsync(IEnumerable<IMigration> previousMigrations)
        {
            if (!previousMigrations.Any(x => x is Migration01_FromCqrs))
            {
                await rebuilder.RebuildContentAsync();
            }
        }
    }
}
