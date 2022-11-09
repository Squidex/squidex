// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Diagnostics;

public sealed class GCHealthCheck : IHealthCheck
{
    private readonly long threshold;

    public GCHealthCheck(IOptions<GCHealthCheckOptions> options)
    {
        threshold = 1024 * 1024 * options.Value.ThresholdInMB;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var workingSet = Process.GetCurrentProcess().WorkingSet64;

        var heapSize = GC.GetTotalMemory(false);

        var data = new Dictionary<string, object>
        {
            { "Gen0CollectionCount", GC.CollectionCount(0) },
            { "Gen1CollectionCount", GC.CollectionCount(1) },
            { "Gen2CollectionCount", GC.CollectionCount(2) },
            { "HeapSizeBytes", heapSize },
            { "HeapSizeString", heapSize.ToReadableSize() },
            { "WorkingSetBytes", workingSet },
            { "WorkingSetString", workingSet.ToReadableSize() }
        };

        var status = workingSet < threshold ?
            HealthStatus.Healthy :
            HealthStatus.Unhealthy;

        var message = $"Application must consume less than {threshold.ToReadableSize()} memory.";

        return Task.FromResult(new HealthCheckResult(status, message, data: data));
    }
}
