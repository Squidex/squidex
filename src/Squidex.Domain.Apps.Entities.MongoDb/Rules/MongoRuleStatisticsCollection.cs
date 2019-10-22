﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleStatisticsCollection : MongoRepositoryBase<RuleStatistics>
    {
        static MongoRuleStatisticsCollection()
        {
            var guidSerializer = new GuidSerializer().WithRepresentation(BsonType.String);

            BsonClassMap.RegisterClassMap<RuleStatistics>(map =>
            {
                map.AutoMap();

                map.MapProperty(x => x.AppId).SetSerializer(guidSerializer);
                map.MapProperty(x => x.RuleId).SetSerializer(guidSerializer);

                map.SetIgnoreExtraElements(true);
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

        protected override Task SetupCollectionAsync(IMongoCollection<RuleStatistics> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<RuleStatistics>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.RuleId)),
                cancellationToken: ct);
        }

        public async Task<IReadOnlyList<RuleStatistics>> QueryByAppAsync(Guid appId)
        {
            var statistics = await Collection.Find(x => x.AppId == appId).ToListAsync();

            return statistics;
        }

        public Task IncrementSuccess(Guid appId, Guid ruleId, Instant now)
        {
            return Collection.UpdateOneAsync(
                x => x.AppId == appId && x.RuleId == ruleId,
                Update
                    .Inc(x => x.NumSucceeded, 1)
                    .Set(x => x.LastExecuted, now)
                    .SetOnInsert(x => x.AppId, appId)
                    .SetOnInsert(x => x.RuleId, ruleId),
                Upsert);
        }

        public Task IncrementFailed(Guid appId, Guid ruleId, Instant now)
        {
            return Collection.UpdateOneAsync(
                x => x.AppId == appId && x.RuleId == ruleId,
                Update
                    .Inc(x => x.NumFailed, 1)
                    .Set(x => x.LastExecuted, now)
                    .SetOnInsert(x => x.AppId, appId)
                    .SetOnInsert(x => x.RuleId, ruleId),
                Upsert);
        }
    }
}
