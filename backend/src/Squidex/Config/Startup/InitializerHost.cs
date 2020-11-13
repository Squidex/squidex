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
    public sealed class InitializerHost : SafeHostedService
    {
        private readonly IEnumerable<IInitializable> targets;

        public InitializerHost(IEnumerable<IInitializable> targets, ISemanticLog log)
            : base(log)
        {
            this.targets = targets;
        }

        protected override async Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            foreach (var target in targets.Distinct().OrderBy(x => x.Order))
            {
                await target.InitializeAsync(ct);

                log.LogInformation(w => w.WriteProperty("initializedSystem", target.ToString()));
            }
        }
    }
}
