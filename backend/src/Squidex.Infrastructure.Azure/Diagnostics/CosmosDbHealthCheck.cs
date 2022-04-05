// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class CosmosDbHealthCheck : IHealthCheck
    {
        private readonly DocumentClient documentClient;

        public CosmosDbHealthCheck(Uri uri, string masterKey)
        {
            documentClient = new DocumentClient(uri, masterKey);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            await documentClient.ReadDatabaseFeedAsync();

            return HealthCheckResult.Healthy("Application must query data from CosmosDB.");
        }
    }
}
