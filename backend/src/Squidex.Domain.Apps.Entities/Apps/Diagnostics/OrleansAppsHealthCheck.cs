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
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps.Diagnostics
{
    public sealed class OrleansAppsHealthCheck : IHealthCheck
    {
        private readonly IGrainFactory grainFactory;

        public OrleansAppsHealthCheck(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            await GetGrain().GetAppIdsAsync(new[] { "test" });

            return HealthCheckResult.Healthy("Orleans must establish communication.");
        }

        private IAppsCacheGrain GetGrain()
        {
            return grainFactory.GetGrain<IAppsCacheGrain>(SingleGrain.Id);
        }
    }
}
