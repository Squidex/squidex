// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Migrations;
using Squidex.Log;

namespace Squidex.Config.Startup
{
    public sealed class MigrationRebuilderHost : SafeHostedService
    {
        private readonly RebuildRunner rebuildRunner;

        public MigrationRebuilderHost(RebuildRunner rebuildRunner, ISemanticLog log)
            : base(log)
        {
            this.rebuildRunner = rebuildRunner;
        }

        protected override Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            return rebuildRunner.RunAsync(ct);
        }
    }
}
