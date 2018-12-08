// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class OrleansHealthCheck : IHealthCheck
    {
        private readonly IManagementGrain managementGrain;

        public IEnumerable<string> Scopes
        {
            get { yield return HealthCheckScopes.Cluster; }
        }

        public OrleansHealthCheck(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var activationCount = await managementGrain.GetTotalActivationCount();

            return new HealthCheckResult(activationCount > 0, "Orleans must have at least one activation.");
        }
    }
}
