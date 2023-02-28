// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Squidex.Infrastructure.EventSourcing.Consume;

public sealed class EventConsumersHealthCheck : IHealthCheck
{
    private readonly IEventConsumerManager eventConsumerManager;

    public EventConsumersHealthCheck(IEventConsumerManager eventConsumerManager)
    {
        this.eventConsumerManager = eventConsumerManager;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var eventConsumers = await eventConsumerManager.GetConsumersAsync(cancellationToken);

        var data = new Dictionary<string, object>();

        var numTotal = 0;
        var numFailed = 0;

        foreach (var eventConsumer in eventConsumers)
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
}
