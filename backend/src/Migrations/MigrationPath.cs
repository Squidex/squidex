// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Migrations.Migrations;
using Migrations.Migrations.MongoDb;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Migrations.Backup;

namespace Migrations;

public sealed class MigrationPath(IServiceProvider serviceProvider) : IMigrationPath
{
    private const int CurrentVersion = 27;

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
        // Version 06: Convert Event store. Must always be executed first.
        if (version < 6)
        {
            yield return serviceProvider.GetService<ConvertEventStore>();
        }

        // Version 22: Integrate Domain Id.
        if (version < 22)
        {
            yield return serviceProvider.GetService<AddAppIdToEventStream>();
        }

        // Version 07: Introduces AppId for backups.
        else if (version < 7)
        {
            yield return serviceProvider.GetService<ConvertEventStoreAppId>();
        }

        // Version 05: Fixes the broken command architecture and requires a rebuild of all snapshots.
        if (version < 5)
        {
            yield return serviceProvider.GetService<RebuildSnapshots>();
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
            // Version 25: Introduce full deletion.
            if (version < 25)
            {
                yield return serviceProvider.GetService<RebuildApps>();
                yield return serviceProvider.GetService<RebuildSchemas>();
                yield return serviceProvider.GetService<RebuildRules>();
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
                    yield return serviceProvider.GetService<ConvertDocumentIds>()?.ForAssets();
                }
            }

            // Version 21: Introduce content drafts V2.
            // Version 25: Convert content ids to names.
            if (version < 25)
            {
                yield return serviceProvider.GetService<RebuildContents>();
            }

            // Version 16: Introduce file name slugs for assets.
            if (version < 16)
            {
                yield return serviceProvider.GetService<CreateAssetSlugs>();
            }
        }

        // Version 13: Json refactoring.
        if (version < 13)
        {
            yield return serviceProvider.GetService<ConvertRuleEventsJson>();
        }

        // Version 26: New rule statistics using normal usage collection.
        if (version < 26)
        {
            yield return serviceProvider.GetService<CopyRuleStatistics>();
        }

        // Version 27: General jobs state.
        if (version < 27)
        {
            yield return serviceProvider.GetService<ConvertBackup>();
        }
    }
}
