﻿// ==========================================================================
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
using Squidex.Infrastructure.Log.Store;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Log
{
    public sealed class MongoRequestLogRepository : MongoRepositoryBase<MongoRequest>, IRequestLogRepository
    {
        private static readonly InsertManyOptions Unordered = new InsertManyOptions { IsOrdered = false };
        private readonly RequestLogStoreOptions options;

        public MongoRequestLogRepository(IMongoDatabase database, IOptions<RequestLogStoreOptions> options)
            : base(database)
        {
            Guard.NotNull(options);

            this.options = options.Value;
        }

        protected override string CollectionName()
        {
            return "RequestLog";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRequest> collection, CancellationToken ct = default)
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
                    }),
            }, ct);
        }

        public Task InsertManyAsync(IEnumerable<Request> items)
        {
            Guard.NotNull(items);

            var documents = items.Select(x => new MongoRequest { Key = x.Key, Timestamp = x.Timestamp, Properties = x.Properties });

            return Collection.InsertManyAsync(documents, Unordered);
        }

        public Task QueryAllAsync(Func<Request, Task> callback, string key, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            Guard.NotNull(callback);
            Guard.NotNullOrEmpty(key);

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
