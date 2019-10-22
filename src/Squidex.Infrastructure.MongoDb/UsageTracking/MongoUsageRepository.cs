﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class MongoUsageRepository : MongoRepositoryBase<MongoUsage>, IUsageRepository
    {
        private static readonly BulkWriteOptions Unordered = new BulkWriteOptions { IsOrdered = false };

        public MongoUsageRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "UsagesV2";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoUsage> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoUsage>(
                    Index
                        .Ascending(x => x.Key)
                        .Ascending(x => x.Category)
                        .Ascending(x => x.Date)),
                cancellationToken: ct);
        }

        public async Task TrackUsagesAsync(UsageUpdate update)
        {
            Guard.NotNull(update, nameof(update));

            if (update.Counters.Count > 0)
            {
                var (filter, updateStatement) = CreateOperation(update);

                await Collection.UpdateOneAsync(filter, updateStatement, Upsert);
            }
        }

        public async Task TrackUsagesAsync(params UsageUpdate[] updates)
        {
            if (updates.Length == 1)
            {
                await TrackUsagesAsync(updates[0]);
            }
            else if (updates.Length > 0)
            {
                var writes = new List<WriteModel<MongoUsage>>();

                foreach (var update in updates)
                {
                    if (update.Counters.Count > 0)
                    {
                        var (filter, updateStatement) = CreateOperation(update);

                        writes.Add(new UpdateOneModel<MongoUsage>(filter, updateStatement) { IsUpsert = true });
                    }
                }

                await Collection.BulkWriteAsync(writes, Unordered);
            }
        }

        private static (FilterDefinition<MongoUsage>, UpdateDefinition<MongoUsage>) CreateOperation(UsageUpdate usageUpdate)
        {
            var id = $"{usageUpdate.Key}_{usageUpdate.Date:yyyy-MM-dd}_{usageUpdate.Category}";

            var update = Update
                .SetOnInsert(x => x.Key, usageUpdate.Key)
                .SetOnInsert(x => x.Date, usageUpdate.Date)
                .SetOnInsert(x => x.Category, usageUpdate.Category);

            foreach (var counter in usageUpdate.Counters)
            {
                update = update.Inc($"Counters.{counter.Key}", counter.Value);
            }

            var filter = Filter.Eq(x => x.Id, id);

            return (filter, update);
        }

        public async Task<IReadOnlyList<StoredUsage>> QueryAsync(string key, DateTime fromDate, DateTime toDate)
        {
            var entities = await Collection.Find(x => x.Key == key && x.Date >= fromDate && x.Date <= toDate).ToListAsync();

            return entities.Select(x => new StoredUsage(x.Category, x.Date, x.Counters)).ToList();
        }
    }
}
