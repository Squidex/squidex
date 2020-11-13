// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.

// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Log;

namespace Squidex.Config.Startup
{
    public sealed class BackgroundHost : SafeHostedService
    {
        private readonly IEnumerable<IBackgroundProcess> targets;

        public BackgroundHost(IEnumerable<IBackgroundProcess> targets, ISemanticLog log)
            : base(log)
        {
            this.targets = targets;
        }

        protected override async Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            foreach (var target in targets.Distinct())
            {
                await target.StartAsync(ct);

                log.LogInformation(w => w.WriteProperty("backgroundSystem", target.ToString()));
            }
        }
    }
}
