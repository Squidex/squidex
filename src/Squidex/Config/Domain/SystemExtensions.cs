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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Domain
{
    public static class SystemExtensions
    {
        public sealed class InitializeHostedService : IHostedService
        {
            private readonly IEnumerable<IInitializable> targets;
            private readonly IApplicationLifetime lifetime;
            private readonly ISemanticLog log;

            public InitializeHostedService(IEnumerable<IInitializable> targets, IApplicationLifetime lifetime, ISemanticLog log)
            {
                this.targets = targets;
                this.lifetime = lifetime;
                this.log = log;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                try
                {
                    foreach (var target in targets)
                    {
                        await target.InitializeAsync(cancellationToken);

                        log.LogInformation(w => w.WriteProperty("initializedSystem", target.GetType().Name));
                    }
                }
                catch
                {
                    lifetime.StopApplication();
                    throw;
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        public sealed class MigratorHostedService : IHostedService
        {
            private readonly IApplicationLifetime lifetime;
            private readonly Migrator migrator;

            public MigratorHostedService(IApplicationLifetime lifetime, Migrator migrator)
            {
                this.lifetime = lifetime;
                this.migrator = migrator;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await migrator.MigrateAsync();
                }
                catch
                {
                    lifetime.StopApplication();
                    throw;
                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
