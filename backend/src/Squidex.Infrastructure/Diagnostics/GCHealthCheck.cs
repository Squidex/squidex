// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class GCHealthCheck : IHealthCheck
    {
        private readonly long threshold;

        public GCHealthCheck(IOptions<GCHealthCheckOptions> options)
        {
            threshold = 1024 * 1024 * options.Value.Threshold;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(false);

            var data = new Dictionary<string, object>
            {
                { "Allocated", allocated.ToReadableSize() },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) }
            };

            var status = allocated < threshold ? HealthStatus.Healthy : HealthStatus.Unhealthy;

            var message = $"Application must consume less than {threshold.ToReadableSize()} memory.";

            return Task.FromResult(new HealthCheckResult(status, message, data: data));
        }
    }
}
