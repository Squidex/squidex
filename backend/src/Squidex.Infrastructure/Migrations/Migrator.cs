// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Migrations;

public sealed partial class Migrator(
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

                    LogMigrationStarted(log, name);

                    try
                    {
                        var watch = ValueStopwatch.StartNew();

                        await migration.UpdateAsync(ct);

                        LogMigrationCompleted(log, name, watch.Stop());
                    }
                    catch (Exception ex)
                    {
                        LogMigrationFailed(log, name, ex);
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
                LogMigrationLockRetry(log, LockWaitMs);
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

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Migration {migration} started.")]
    private static partial void LogMigrationStarted(ILogger logger, string migration);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Migration {migration} completed after {time}ms.")]
    private static partial void LogMigrationCompleted(ILogger logger, string migration, long time);

    [LoggerMessage(EventId = 3, Level = LogLevel.Critical, Message = "Migration {migration} failed.")]
    private static partial void LogMigrationFailed(ILogger logger, string migration, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Could not acquire lock to start migrating. Trying again in {time}ms.")]
    private static partial void LogMigrationLockRetry(ILogger logger, int time);
}
