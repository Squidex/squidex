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
using Microsoft.Extensions.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Startup
{
    public sealed class InitializerHost : SafeHostedService
    {
        private readonly IEnumerable<IInitializable> targets;

        public InitializerHost(IEnumerable<IInitializable> targets, IApplicationLifetime lifetime, ISemanticLog log)
            : base(lifetime, log)
        {
            this.targets = targets;
        }

        protected override async Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            foreach (var target in targets.Distinct())
            {
                await target.InitializeAsync(ct);

                log.LogInformation(w => w.WriteProperty("initializedSystem", target.GetType().Name));
            }
        }
    }
}
