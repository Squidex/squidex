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
using Migrations.Migrations;
using Migrations.Migrations.MongoDb;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Migrations
{
    public sealed class MigrationPath : IMigrationPath
    {
        private const int CurrentVersion = 26;
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

            var migrations = ResolveMigrators(version).NotNull().ToList();

            return (CurrentVersion, migrations);
        }

        private IEnumerable<IMigration?> ResolveMigrators(int version)
        {
            yield return serviceProvider.GetRequiredService<StopEventConsumers>();

            // Version 06: Convert Event store. Must always be executed first.
            if (version < 6)
            {
                yield return serviceProvider.GetRequiredService<ConvertEventStore>();
            }

            // Version 22: Integrate Domain Id.
            if (version < 22)
            {
                yield return serviceProvider.GetRequiredService<AddAppIdToEventStream>();
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
            else
            {
                // Version 09: Grain indexes.
                if (version < 9)
                {
                    yield return serviceProvider.GetService<ConvertOldSnapshotStores>();
                }

                // Version 12: Introduce roles.
                // Version 24: Improve a naming in the languages config.
                // Version 26: Introduce full deletion.
                if (version < 26)
                {
                    // yield return serviceProvider.GetRequiredService<RebuildApps>();
                    // yield return serviceProvider.GetRequiredService<RebuildSchemas>();
                    yield return serviceProvider.GetRequiredService<RebuildRules>();
                }

                // Version 18: Rebuild assets.
                if (version < 18)
                {
                    yield return serviceProvider.GetService<RebuildAssetFolders>();
                    yield return serviceProvider.GetService<RebuildAssets>();
                }
                else
                {
                    // Version 20: Rename slug field.
                    if (version < 20)
                    {
                        yield return serviceProvider.GetService<RenameAssetMetadata>();
                    }

                    // Version 22: Introduce domain id.
                    // Version 23: Fix parent id.
                    if (version < 23)
                    {
                        yield return serviceProvider.GetRequiredService<ConvertDocumentIds>().ForAssets();
                    }
                }

                // Version 21: Introduce content drafts V2.
                // Version 25: Convert content ids to names.
                if (version < 25)
                {
                    yield return serviceProvider.GetRequiredService<RebuildContents>();
                }

                // Version 16: Introduce file name slugs for assets.
                if (version < 16)
                {
                    yield return serviceProvider.GetRequiredService<CreateAssetSlugs>();
                }
            }

            // Version 13: Json refactoring
            if (version < 13)
            {
                yield return serviceProvider.GetRequiredService<ConvertRuleEventsJson>();
            }

            yield return serviceProvider.GetRequiredService<StartEventConsumers>();
        }
    }
}
