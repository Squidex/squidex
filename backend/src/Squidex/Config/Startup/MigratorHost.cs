// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Startup;

public sealed class MigratorHost : IHostedService
{
    private readonly Migrator migrator;

    public MigratorHost(Migrator migrator)
    {
        this.migrator = migrator;
    }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        return migrator.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
