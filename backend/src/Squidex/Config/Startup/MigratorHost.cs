// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Startup;

public sealed class MigratorHost(Migrator migrator) : IHostedService
{
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
