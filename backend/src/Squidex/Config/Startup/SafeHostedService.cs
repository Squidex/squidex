// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Squidex.Log;

namespace Squidex.Config.Startup
{
    public abstract class SafeHostedService : IHostedService
    {
        private readonly ISemanticLog log;
        private bool isStarted;

        protected SafeHostedService(ISemanticLog log)
        {
            this.log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartAsync(log, cancellationToken);

            isStarted = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (isStarted)
            {
                await StopAsync(log, cancellationToken);
            }
        }

        protected abstract Task StartAsync(ISemanticLog log, CancellationToken ct);

        protected virtual Task StopAsync(ISemanticLog log, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
