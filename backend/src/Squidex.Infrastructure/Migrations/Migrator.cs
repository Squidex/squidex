// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Migrations;

public sealed class Migrator(
    IMigrationStatus migrationStatus,
    IMigrationPath migrationPath,
    ILogger<Migrator> log)
{
    public int LockWaitMs { get; set; } = 500;

    public async Task MigrateAsync(
        CancellationToken ct = default)
    {
        if (!await TryLockAsync(ct))
        {
            return;
        }

        try
        {
            var version = await migrationStatus.GetVersionAsync(ct);

            while (!ct.IsCancellationRequested)
            {
                var (newVersion, migrations) = migrationPath.GetNext(version);

                if (migrations == null || !migrations.Any())
                {
                    break;
                }

                foreach (var migration in migrations)
                {
                    var name = migration.ToString()!;

                    LogMessages.LogMigrationStarted(log, name);

                    try
                    {
                        var watch = ValueStopwatch.StartNew();

                        await migration.UpdateAsync(ct);

                        LogMessages.LogMigrationCompleted(log, name, watch.Stop());
                    }
                    catch (Exception ex)
                    {
                        LogMessages.LogMigrationFailed(log, name, ex);
                        throw new MigrationFailedException(name, ex);
                    }
                }

                version = newVersion;

                await migrationStatus.CompleteAsync(newVersion, ct);
            }
        }
        finally
        {
            await UnlockAsync();
        }
    }

    private async Task<bool> TryLockAsync(
        CancellationToken ct)
    {
        try
        {
            while (!await migrationStatus.TryLockAsync(ct))
            {
                LogMessages.LogMigrationLockRetry(log, LockWaitMs);
                await Task.Delay(LockWaitMs, ct);
            }
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        return true;
    }

    private Task UnlockAsync()
    {
        return migrationStatus.UnlockAsync();
    }
}
