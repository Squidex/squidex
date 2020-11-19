﻿// ==========================================================================
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
        private const int CurrentVersion = 23;
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
                if (version < 12)
                {
                    yield return serviceProvider.GetRequiredService<RebuildApps>();
                }

                // Version 14: Schema refactoring
                // Version 22: Introduce domain id.
                if (version < 22)
                {
                    yield return serviceProvider.GetRequiredService<ClearSchemas>();
                    yield return serviceProvider.GetRequiredService<ClearRules>();
                }

                // Version 18: Rebuild assets.
                if (version < 18)
                {
                    yield return serviceProvider.GetService<RebuildAssetFolders>();
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

                    // Version 22: Introduce domain id.
                    // Version 23: Fix parent id.
                    if (version < 23)
                    {
                        yield return serviceProvider.GetRequiredService<ConvertDocumentIds>().ForAssets();
                    }
                }

                // Version 21: Introduce content drafts V2.
                if (version < 21)
                {
                    yield return serviceProvider.GetRequiredService<RebuildContents>();
                }
                else
                {
                    // Version 22: Introduce domain id.
                    if (version < 22)
                    {
                        yield return serviceProvider.GetRequiredService<ConvertDocumentIds>().ForContents();
                    }
                }
            }

            // Version 13: Json refactoring
            if (version < 13)
            {
                yield return serviceProvider.GetRequiredService<ConvertRuleEventsJson>();
            }

            // Version 16: Introduce file name slugs for assets.
            if (version < 16)
            {
                yield return serviceProvider.GetRequiredService<CreateAssetSlugs>();
            }

            // Version 19: Unify indexes.
            if (version < 19)
            {
                yield return serviceProvider.GetRequiredService<PopulateGrainIndexes>();
            }

            yield return serviceProvider.GetRequiredService<StartEventConsumers>();
        }
    }
}
