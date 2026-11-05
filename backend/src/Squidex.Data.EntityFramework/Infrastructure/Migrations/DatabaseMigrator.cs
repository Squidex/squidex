// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Squidex.Hosting;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Migrations;

public sealed class DatabaseMigrator<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    ConnectionStringParser parser,
    ILogger<DatabaseMigrator<TContext>> log)
    : IInitializable
    where TContext : DbContext
{
    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(30);

    public int Order => -1000;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        try
        {
            using var cts = new CancellationTokenSource(WaitTime);
            while (!await dbContext.Database.CanConnectAsync(cts.Token))
            {
                await Task.Delay(100, cts.Token);
            }

            cts.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            var connectionString = dbContext.Database.GetConnectionString();

            var hostName = parser.GetHostName(connectionString);

            if (string.IsNullOrWhiteSpace(hostName))
            {
                hostName = "Unknown";
            }

            throw new InvalidOperationException($"Failed to connect to database <{hostName}>");
        }

        await MigrateAsync(dbContext, ct);
    }

    private async Task MigrateAsync(TContext dbContext,
        CancellationToken ct)
    {
        try
        {
            await dbContext.Database.MigrateAsync(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Some migrations need more privileges than the application, e.g. to create functions or extensions.
            // The pending migrations are written to the log, so that an administrator can apply them manually.
            var sql = await GetPendingScriptAsync(dbContext, ct);
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw;
            }

            var context = typeof(TContext).Name;

            if (dbContext is IDbContextWithDialect withDialect && withDialect.Dialect.IsPrivilegeMissingException(ex))
            {
                LogMessages.LogMigrationPrivilegesMissing(log, ex, context, sql);
            }
            else
            {
                LogMessages.LogMigrationFailed(log, ex, context, sql);
            }

            throw;
        }
    }

    private static async Task<string?> GetPendingScriptAsync(TContext dbContext,
        CancellationToken ct)
    {
        try
        {
            var migrationsApplied = await dbContext.Database.GetAppliedMigrationsAsync(ct);
            var migrationsLast = migrationsApplied.Order(StringComparer.Ordinal).LastOrDefault();

            // Failed migrations are not recorded, therefore the script starts after the last applied migration.
            return dbContext.GetService<IMigrator>().GenerateScript(migrationsLast ?? Migration.InitialDatabase);
        }
        catch
        {
            // The script is only an additional hint and must not hide the original exception.
            return null;
        }
    }
}
