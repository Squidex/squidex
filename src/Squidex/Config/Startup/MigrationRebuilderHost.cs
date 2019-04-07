// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Migrate_01;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Startup
{
    public sealed class MigrationRebuilderHost : SafeHostedService
    {
        private readonly RebuildRunner rebuildRunner;

        public MigrationRebuilderHost(IApplicationLifetime lifetime, ISemanticLog log, RebuildRunner rebuildRunner)
            : base(lifetime, log)
        {
            this.rebuildRunner = rebuildRunner;
        }

        protected override Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            return rebuildRunner.RunAsync(ct);
        }
    }
}
