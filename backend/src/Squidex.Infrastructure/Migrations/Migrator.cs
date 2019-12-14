﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
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
            Guard.NotNull(migrationStatus);
            Guard.NotNull(migrationPath);
            Guard.NotNull(log);

            this.migrationStatus = migrationStatus;
            this.migrationPath = migrationPath;

            this.log = log;
        }

        public async Task MigrateAsync(CancellationToken ct = default)
        {
            var version = 0;

            try
            {
                while (!await migrationStatus.TryLockAsync())
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "Migrate")
                        .WriteProperty("mesage", $"Waiting {LockWaitMs}ms to acquire lock."));

                    await Task.Delay(LockWaitMs, ct);
                }

                version = await migrationStatus.GetVersionAsync();

                while (!ct.IsCancellationRequested)
                {
                    var (newVersion, migrations) = migrationPath.GetNext(version);

                    if (migrations == null || !migrations.Any())
                    {
                        break;
                    }

                    foreach (var migration in migrations)
                    {
                        var name = migration.GetType().ToString();

                        log.LogInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Started")
                            .WriteProperty("migrator", name));

                        try
                        {
                            using (log.MeasureInformation(w => w
                                .WriteProperty("action", "Migration")
                                .WriteProperty("status", "Completed")
                                .WriteProperty("migrator", name)))
                            {
                                await migration.UpdateAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogFatal(ex, w => w
                                .WriteProperty("action", "Migration")
                                .WriteProperty("status", "Failed")
                                .WriteProperty("migrator", name));

                            throw new MigrationFailedException(name, ex);
                        }
                    }

                    version = newVersion;
                }
            }
            finally
            {
                await migrationStatus.UnlockAsync(version);
            }
        }
    }
}
