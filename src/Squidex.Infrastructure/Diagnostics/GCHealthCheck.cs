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
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class GCHealthCheck : IHealthCheck
    {
        private readonly long threshold;

        public IEnumerable<string> Scopes
        {
            get { yield return HealthCheckScopes.Node; }
        }

        public GCHealthCheck(IOptions<GCHealthCheckOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            threshold = 1024 * 1024 * options.Value.Threshold;
        }

        public Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var allocated = GC.GetTotalMemory(false);

            var data = new Dictionary<string, object>()
            {
                { "Allocated", allocated },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) },
            };

            return Task.FromResult(new HealthCheckResult(allocated < threshold, $"Reports degraded status if allocated bytes >= {threshold.ToReadableSize()}", data));
        }
    }
}
