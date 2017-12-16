// ==========================================================================
//  Migrator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class Migrator
    {
        private readonly IMigrationStatus migrationStatus;
        private readonly IEnumerable<IMigration> migrations;
        private readonly ISemanticLog log;

        public int LockWaitMs { get; set; } = 5000;

        public Migrator(IMigrationStatus migrationStatus, IEnumerable<IMigration> migrations, ISemanticLog log)
        {
            Guard.NotNull(migrationStatus, nameof(migrationStatus));
            Guard.NotNull(migrations, nameof(migrations));
            Guard.NotNull(log, nameof(log));

            this.migrationStatus = migrationStatus;
            this.migrations = migrations.OrderByDescending(x => x.ToVersion).ToList();

            this.log = log;
        }

        public async Task MigrateAsync()
        {
            var version = 0;

            try
            {
                var lastMigrator = migrations.FirstOrDefault();

                while (!await migrationStatus.TryLockAsync(lastMigrator.ToVersion))
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "Migrate")
                        .WriteProperty("mesage", $"Waiting {LockWaitMs}ms to acquire lock."));

                    await Task.Delay(LockWaitMs);
                }

                version = await migrationStatus.GetVersionAsync();

                if (lastMigrator != null && lastMigrator.ToVersion != version)
                {
                    var migrationPath = FindMigratorPath(version, lastMigrator.ToVersion).ToList();

                    foreach (var migrator in migrationPath)
                    {
                        var name = migrator.GetType().ToString();

                        log.LogInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Started")
                            .WriteProperty("migrator", name));

                        using (log.MeasureInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Completed")
                            .WriteProperty("migrator", name)))
                        {
                            await migrator.UpdateAsync();

                            version = migrator.ToVersion;
                        }
                    }
                }
            }
            finally
            {
                await migrationStatus.UnlockAsync(version);
            }
        }

        private IEnumerable<IMigration> FindMigratorPath(int fromVersion, int toVersion)
        {
            var addedMigrators = new HashSet<IMigration>();

            while (true)
            {
                var bestMigrator = migrations.Where(x => x.FromVersion < x.ToVersion).FirstOrDefault(x => x.FromVersion == fromVersion);

                if (bestMigrator != null && addedMigrators.Add(bestMigrator))
                {
                    fromVersion = bestMigrator.ToVersion;

                    yield return bestMigrator;
                }
                else if (fromVersion != toVersion)
                {
                    throw new InvalidOperationException($"There is no migration path from {fromVersion} to {toVersion}.");
                }
                else
                {
                    break;
                }
            }
        }
    }
}
