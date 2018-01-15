// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class Migrator
    {
        private readonly ISemanticLog log;
        private readonly IMigrationStatus migrationStatus;
        private readonly IMigrationPath migrationPath;

        public int LockWaitMs { get; set; } = 500;

        public Migrator(IMigrationStatus migrationStatus, IMigrationPath migrationPath, ISemanticLog log)
        {
            Guard.NotNull(migrationStatus, nameof(migrationStatus));
            Guard.NotNull(migrationPath, nameof(migrationPath));
            Guard.NotNull(log, nameof(log));

            this.migrationStatus = migrationStatus;
            this.migrationPath = migrationPath;

            this.log = log;
        }

        public async Task MigrateAsync()
        {
            var version = 0;

            try
            {
                while (!await migrationStatus.TryLockAsync())
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "Migrate")
                        .WriteProperty("mesage", $"Waiting {LockWaitMs}ms to acquire lock."));

                    await Task.Delay(LockWaitMs);
                }

                version = await migrationStatus.GetVersionAsync();

                while (true)
                {
                    var migrationStep = migrationPath.GetNext(version);

                    if (migrationStep.Migrations == null || !migrationStep.Migrations.Any())
                    {
                        break;
                    }

                    foreach (var migration in migrationStep.Migrations)
                    {
                        var name = migration.GetType().ToString();

                        log.LogInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Started")
                            .WriteProperty("migrator", name));

                        using (log.MeasureInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Completed")
                            .WriteProperty("migrator", name)))
                        {
                            await migration.UpdateAsync();
                        }
                    }

                    version = migrationStep.Version;
                }
            }
            finally
            {
                await migrationStatus.UnlockAsync(version);
            }
        }
    }
}
