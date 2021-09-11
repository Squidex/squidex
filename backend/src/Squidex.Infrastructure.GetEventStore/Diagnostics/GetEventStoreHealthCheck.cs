// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class GetEventStoreHealthCheck : IHealthCheck
    {
        private readonly IEventStoreConnection connection;

        public GetEventStoreHealthCheck(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            await connection.ReadEventAsync("test", 1, false);

            return HealthCheckResult.Healthy("Application must query data from EventStore.");
        }
    }
}
