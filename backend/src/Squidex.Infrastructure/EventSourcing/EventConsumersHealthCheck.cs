// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class EventConsumersHealthCheck : IHealthCheck
    {
        private readonly IGrainFactory grainFactory;

        public EventConsumersHealthCheck(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var eventConsumers = await GetGrain().GetConsumersAsync();

            var data = new Dictionary<string, object>();

            var numTotal = 0;
            var numFailed = 0;

            foreach (var eventConsumer in eventConsumers.Value)
            {
                var status = "Running";

                if (eventConsumer.Error != null)
                {
                    status = "Failed";

                    numFailed++;
                }
                else if (eventConsumer.IsStopped)
                {
                    status = "Stopped";
                }

                data[eventConsumer.Name] = status;

                numTotal++;
            }

            if (numTotal > 0 && numFailed == numTotal)
            {
                return HealthCheckResult.Unhealthy("All event consumers failed", null, data);
            }
            else if (numFailed > 0)
            {
                return HealthCheckResult.Degraded("One or more event consumers failed", null, data);
            }
            else
            {
                return HealthCheckResult.Healthy(data: data);
            }
        }

        private IEventConsumerManagerGrain GetGrain()
        {
            return grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }
    }
}
