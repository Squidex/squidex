// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Squidex.Config.Startup;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Config.Orleans
{
    public sealed class SiloHost : SafeHostedService
    {
        private readonly ISiloHost silo;

        public SiloHost(ISiloHost silo, ISemanticLog log, IApplicationLifetime lifetime)
            : base(lifetime, log)
        {
            this.silo = silo;
        }

        protected override async Task StartAsync(ISemanticLog log, CancellationToken ct)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await silo.StartAsync(ct);
            }
            finally
            {
                var elapsedMs = watch.Stop();

                log.LogInformation(w => w
                    .WriteProperty("message", "Silo started")
                    .WriteProperty("elapsedMs", elapsedMs));
            }
        }

        protected override async Task StopAsync(ISemanticLog log, CancellationToken ct)
        {
            await silo.StopAsync();
        }
    }
}
