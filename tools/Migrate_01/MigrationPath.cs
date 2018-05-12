// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Migrate_01.Migrations;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public sealed class MigrationPath : IMigrationPath
    {
        private const int CurrentVersion = 11;
        private readonly IServiceProvider serviceProvider;

        public MigrationPath(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public (int Version, IEnumerable<IMigration> Migrations) GetNext(int version)
        {
            if (version == CurrentVersion)
            {
                return (CurrentVersion, null);
            }

            var migrations = new List<IMigration>();

            // Version 06: Convert Event store. Must always be executed first.
            if (version < 6)
            {
                migrations.Add(serviceProvider.GetRequiredService<ConvertEventStore>());
            }

            // Version 07: Introduces AppId for backups.
            else if (version < 7)
            {
                migrations.Add(serviceProvider.GetRequiredService<ConvertEventStoreAppId>());
            }

            // Version 05: Fixes the broken command architecture and requires a rebuild of all snapshots.
            if (version < 5)
            {
                migrations.Add(serviceProvider.GetRequiredService<RebuildSnapshots>());
            }

            // Version 09: Grain indexes.
            if (version < 9)
            {
                var migration = serviceProvider.GetService<ConvertOldSnapshotStores>();

                if (migration != null)
                {
                    migrations.Add(migration);
                }

                migrations.Add(serviceProvider.GetRequiredService<PopulateGrainIndexes>());
            }

            // Version 11: Introduce content drafts.
            if (version < 11)
            {
                var migration = serviceProvider.GetService<DeleteContentCollections>();

                if (migration != null)
                {
                    migrations.Add(migration);
                }

                migrations.Add(serviceProvider.GetRequiredService<RebuildContents>());
            }

            // Version 01: Introduce app patterns.
            if (version < 1)
            {
                migrations.Add(serviceProvider.GetRequiredService<AddPatterns>());
            }

            return (CurrentVersion, migrations);
        }
    }
}
