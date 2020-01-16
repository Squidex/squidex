// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Migrate_01.Migrations;
using Migrate_01.Migrations.MongoDb;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public sealed class MigrationPath : IMigrationPath
    {
        private const int CurrentVersion = 20;
        private readonly IServiceProvider serviceProvider;

        public MigrationPath(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public (int Version, IEnumerable<IMigration>? Migrations) GetNext(int version)
        {
            if (version == CurrentVersion)
            {
                return (CurrentVersion, null);
            }

            var migrations = ResolveMigrators(version).Where(x => x != null).ToList();

            return (CurrentVersion, migrations);
        }

        private IEnumerable<IMigration> ResolveMigrators(int version)
        {
            yield return serviceProvider.GetRequiredService<StopEventConsumers>();

            // Version 06: Convert Event store. Must always be executed first.
            if (version < 6)
            {
                yield return serviceProvider.GetRequiredService<ConvertEventStore>();
            }

            // Version 07: Introduces AppId for backups.
            else if (version < 7)
            {
                yield return serviceProvider.GetRequiredService<ConvertEventStoreAppId>();
            }

            // Version 05: Fixes the broken command architecture and requires a rebuild of all snapshots.
            if (version < 5)
            {
                yield return serviceProvider.GetRequiredService<RebuildSnapshots>();
            }

            // Version 12: Introduce roles.
            else if (version < 12)
            {
                yield return serviceProvider.GetRequiredService<RebuildApps>();
            }

            // Version 09: Grain indexes.
            if (version < 9)
            {
                yield return serviceProvider.GetService<ConvertOldSnapshotStores>();
            }

            // Version 19: Unify indexes.
            if (version < 19)
            {
                yield return serviceProvider.GetRequiredService<PopulateGrainIndexes>();
            }

            // Version 11: Introduce content drafts.
            if (version < 11)
            {
                yield return serviceProvider.GetService<DeleteContentCollections>();
                yield return serviceProvider.GetRequiredService<RebuildContents>();
            }

            // Version 13: Json refactoring
            if (version < 13)
            {
                yield return serviceProvider.GetRequiredService<ConvertRuleEventsJson>();
            }

            // Version 14: Schema refactoring
            if (version < 14)
            {
                yield return serviceProvider.GetRequiredService<ClearSchemas>();
            }

            // Version 01: Introduce app patterns.
            if (version < 1)
            {
                yield return serviceProvider.GetRequiredService<AddPatterns>();
            }

            // Version 15: Introduce custom full text search actors.
            if (version < 15)
            {
                yield return serviceProvider.GetRequiredService<RestructureContentCollection>();
            }

            // Version 18: Rebuild assets.
            if (version < 18)
            {
                yield return serviceProvider.GetService<RebuildAssets>();
            }
            else
            {
                // Version 17: Rename slug field.
                if (version < 17)
                {
                    yield return serviceProvider.GetService<RenameAssetSlugField>();
                }

                // Version 20: Rename slug field.
                if (version < 20)
                {
                    yield return serviceProvider.GetService<RenameAssetMetadata>();
                }
            }

            // Version 16: Introduce file name slugs for assets.
            if (version < 16)
            {
                yield return serviceProvider.GetRequiredService<CreateAssetSlugs>();
            }

            yield return serviceProvider.GetRequiredService<StartEventConsumers>();
        }
    }
}
