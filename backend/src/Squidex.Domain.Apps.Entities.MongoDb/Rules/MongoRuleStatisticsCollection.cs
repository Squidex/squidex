// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleStatisticsCollection : MongoRepositoryBase<RuleStatistics>
    {
        static MongoRuleStatisticsCollection()
        {
            BsonClassMap.RegisterClassMap<RuleStatistics>(cm =>
            {
                cm.AutoMap();

                cm.SetIgnoreExtraElements(true);
            });
        }

        public MongoRuleStatisticsCollection(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "RuleStatistics";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<RuleStatistics> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<RuleStatistics>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.RuleId)),
                cancellationToken: ct);
        }

        public async Task DeleteAppAsync(DomainId appId,
            CancellationToken ct)
        {
            await Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, appId), ct);
        }

        public async Task<IReadOnlyList<RuleStatistics>> QueryByAppAsync(DomainId appId,
            CancellationToken ct)
        {
            var statistics = await Collection.Find(x => x.AppId == appId).ToListAsync(ct);

            return statistics;
        }

        public Task IncrementSuccessAsync(DomainId appId, DomainId ruleId, Instant now,
            CancellationToken ct)
        {
            return Collection.UpdateOneAsync(
                x => x.AppId == appId && x.RuleId == ruleId,
                Update
                    .Inc(x => x.NumSucceeded, 1)
                    .Set(x => x.LastExecuted, now)
                    .SetOnInsert(x => x.AppId, appId)
                    .SetOnInsert(x => x.RuleId, ruleId),
                Upsert, ct);
        }

        public Task IncrementFailedAsync(DomainId appId, DomainId ruleId, Instant now,
            CancellationToken ct)
        {
            return Collection.UpdateOneAsync(
                x => x.AppId == appId && x.RuleId == ruleId,
                Update
                    .Inc(x => x.NumFailed, 1)
                    .Set(x => x.LastExecuted, now)
                    .SetOnInsert(x => x.AppId, appId)
                    .SetOnInsert(x => x.RuleId, ruleId),
                Upsert, ct);
        }
    }
}
