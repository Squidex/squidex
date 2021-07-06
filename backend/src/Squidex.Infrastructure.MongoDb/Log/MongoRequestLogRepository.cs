// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Log
{
    public sealed class MongoRequestLogRepository : MongoRepositoryBase<MongoRequest>, IRequestLogRepository
    {
        private readonly RequestLogStoreOptions options;

        public MongoRequestLogRepository(IMongoDatabase database, IOptions<RequestLogStoreOptions> options)
            : base(database)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options.Value;
        }

        protected override string CollectionName()
        {
            return "RequestLog";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRequest> collection,
            CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoRequest>(
                    Index
                        .Ascending(x => x.Key)
                        .Ascending(x => x.Timestamp)),
                new CreateIndexModel<MongoRequest>(
                    Index
                        .Ascending(x => x.Timestamp),
                    new CreateIndexOptions
                    {
                        ExpireAfter = TimeSpan.FromDays(options.StoreRetentionInDays)
                    })
            }, ct);
        }

        public Task InsertManyAsync(IEnumerable<Request> items)
        {
            Guard.NotNull(items, nameof(items));

            var entities = items.Select(x => new MongoRequest { Key = x.Key, Timestamp = x.Timestamp, Properties = x.Properties }).ToList();

            if (entities.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.InsertManyAsync(entities, InsertUnordered);
        }

        public Task QueryAllAsync(Func<Request, Task> callback, string key, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.NotNullOrEmpty(key, nameof(key));

            var timestampStart = Instant.FromDateTimeUtc(fromDate);
            var timestampEnd = Instant.FromDateTimeUtc(toDate.AddDays(1));

            return Collection.Find(x => x.Key == key && x.Timestamp >= timestampStart && x.Timestamp < timestampEnd).ForEachAsync(x =>
            {
                var request = new Request { Key = x.Key, Timestamp = x.Timestamp, Properties = x.Properties };

                return callback(request);
            }, ct);
        }
    }
}
