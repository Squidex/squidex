// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Squidex.Infrastructure.Diagnostics;

public sealed class GetEventStoreHealthCheck : IHealthCheck
{
    private readonly EventStoreClient client;

    public GetEventStoreHealthCheck(EventStoreClientSettings settings)
    {
        client = new EventStoreClient(settings);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await client.ReadStreamAsync(Direction.Forwards, "test", default, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        return HealthCheckResult.Healthy("Application must query data from EventStore.");
    }
}
