// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class OrleansHealthCheck : IHealthCheck
    {
        private readonly IManagementGrain managementGrain;

        public OrleansHealthCheck(IGrainFactory grainFactory)
        {
            managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var activationCount = await managementGrain.GetTotalActivationCount();

            var status = activationCount > 0 ?
                HealthStatus.Healthy :
                HealthStatus.Unhealthy;

            return new HealthCheckResult(status, "Orleans must have at least one activation.");
        }
    }
}
