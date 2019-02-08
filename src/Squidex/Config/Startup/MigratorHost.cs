// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Config.Startup
{
    public sealed class MigratorHost : SafeHostedService
    {
        private readonly Migrator migrator;

        public MigratorHost(Migrator migrator, IApplicationLifetime lifetime, ISemanticLog log)
            : base(lifetime, log)
        {
            this.migrator = migrator;
        }

        protected override Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            return migrator.MigrateAsync();
        }
    }
}
