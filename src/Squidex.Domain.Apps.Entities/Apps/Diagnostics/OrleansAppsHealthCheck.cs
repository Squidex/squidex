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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Apps.Diagnostics
{
    public sealed class OrleansAppsHealthCheck : IHealthCheck
    {
        private readonly IAppsByNameIndex index;

        public OrleansAppsHealthCheck(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await index.CountAsync();

            return HealthCheckResult.Healthy("Orleans must establish communication.");
        }
    }
}
