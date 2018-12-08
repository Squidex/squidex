// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Squidex.Infrastructure.Diagnostics
{
    public sealed class MongoDBHealthCheck : IHealthCheck
    {
        private readonly IMongoDatabase mongoDatabase;

        public IEnumerable<string> Scopes
        {
            get { yield return HealthCheckScopes.Node; }
        }

        public MongoDBHealthCheck(IMongoDatabase mongoDatabase)
        {
            Guard.NotNull(mongoDatabase, nameof(mongoDatabase));

            this.mongoDatabase = mongoDatabase;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var collectionNames = await mongoDatabase.ListCollectionNamesAsync(cancellationToken: cancellationToken);

            var result = await collectionNames.AnyAsync(cancellationToken);

            return new HealthCheckResult(result);
        }
    }
}
