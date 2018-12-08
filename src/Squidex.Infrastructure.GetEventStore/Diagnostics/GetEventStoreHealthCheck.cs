// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class GetEventStoreHealthCheck : IHealthCheck
    {
        private readonly IEventStoreConnection connection;

        public IEnumerable<string> Scopes
        {
            get { yield return HealthCheckScopes.Node; }
        }

        public GetEventStoreHealthCheck(IEventStoreConnection connection)
        {
            Guard.NotNull(connection, nameof(connection));

            this.connection = connection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await connection.ReadEventAsync("test", 1, false);

            return new HealthCheckResult(true, "Querying test event from event store.");
        }
    }
}
