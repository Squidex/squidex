// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Migrations;

namespace Squidex.Config.Startup
{
    public sealed class MigrationRebuilderHost : IHostedService
    {
        private readonly RebuildRunner rebuildRunner;

        public MigrationRebuilderHost(RebuildRunner rebuildRunner)
        {
            this.rebuildRunner = rebuildRunner;
        }

        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            return rebuildRunner.RunAsync(cancellationToken);
        }

        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
