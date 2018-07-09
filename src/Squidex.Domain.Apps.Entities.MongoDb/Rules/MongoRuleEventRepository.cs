// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleEventRepository : MongoRepositoryBase<MongoRuleEventEntity>, IRuleEventRepository
    {
        public MongoRuleEventRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "RuleEvents";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoRuleEventEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoRuleEventEntity>(Index.Ascending(x => x.NextAttempt)));
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoRuleEventEntity>(Index.Ascending(x => x.AppId).Descending(x => x.Created)));
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoRuleEventEntity>(Index.Ascending(x => x.Expires), new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }));
        }

        public Task QueryPendingAsync(Instant now, Func<IRuleEventEntity, Task> callback, CancellationToken ct = default(CancellationToken))
        {
            return Collection.Find(x => x.NextAttempt < now).ForEachAsync(callback, ct);
        }

        public async Task<IReadOnlyList<IRuleEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20)
        {
            var ruleEventEntities =
                await Collection.Find(x => x.AppId == appId).Skip(skip).Limit(take).SortByDescending(x => x.Created)
                    .ToListAsync();

            return ruleEventEntities;
        }

        public async Task<IRuleEventEntity> FindAsync(Guid id)
        {
            var ruleEvent =
                await Collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            return ruleEvent;
        }

        public async Task<int> CountByAppAsync(Guid appId)
        {
            return (int)await Collection.CountDocumentsAsync(x => x.AppId == appId);
        }

        public Task EnqueueAsync(Guid id, Instant nextAttempt)
        {
            return Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.NextAttempt, nextAttempt));
        }

        public Task EnqueueAsync(RuleJob job, Instant nextAttempt)
        {
            var entity = SimpleMapper.Map(job, new MongoRuleEventEntity { Id = job.JobId, Job = job, Created = nextAttempt, NextAttempt = nextAttempt });

            return Collection.InsertOneIfNotExistsAsync(entity);
        }

        public Task MarkSentAsync(Guid jobId, string dump, RuleResult result, RuleJobResult jobResult, TimeSpan elapsed, Instant? nextAttempt)
        {
            return Collection.UpdateOneAsync(x => x.Id == jobId,
                Update
                    .Set(x => x.Result, result)
                    .Set(x => x.LastDump, dump)
                    .Set(x => x.JobResult, jobResult)
                    .Set(x => x.NextAttempt, nextAttempt)
                    .Inc(x => x.NumCalls, 1));
        }
    }
}
