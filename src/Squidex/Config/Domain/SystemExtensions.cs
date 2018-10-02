// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Domain
{
    public static class SystemExtensions
    {
        public sealed class InitializeHostedService : IHostedService
        {
            private readonly IEnumerable<IInitializable> targets;

            public InitializeHostedService(IEnumerable<IInitializable> targets)
            {
                this.targets = targets;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                foreach (var target in targets)
                {
                    await target.InitializeAsync(cancellationToken);
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        public sealed class MigratorHostedService : IHostedService
        {
            private readonly Migrator migrator;

            public MigratorHostedService(Migrator migrator)
            {
                this.migrator = migrator;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                return migrator.MigrateAsync();
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
