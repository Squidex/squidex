// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public sealed class Migration01_FromCqrs : IMigration
    {
        private readonly Rebuilder rebuilder;

        public int FromVersion { get; } = 0;

        public int ToVersion { get; } = 1;

        public Migration01_FromCqrs(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
        }

        public async Task UpdateAsync(IEnumerable<IMigration> previousMigrations)
        {
            await rebuilder.RebuildConfigAsync();
            await rebuilder.RebuildContentAsync();
            await rebuilder.RebuildAssetsAsync();
        }
    }
}
