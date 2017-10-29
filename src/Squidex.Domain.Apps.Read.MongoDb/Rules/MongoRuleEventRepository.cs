// ==========================================================================
//  MongoRuleEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Rules
{
    public sealed class MongoRuleEventRepository : MongoRepositoryBase<MongoRuleEventEntity>, IRuleEventRepository
    {
        private readonly IClock clock;

        public MongoRuleEventRepository(IMongoDatabase database, IClock clock)
            : base(database)
        {
            Guard.NotNull(clock, nameof(clock));

            this.clock = clock;
        }

        protected override string CollectionName()
        {
            return "RuleEvents";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRuleEventEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.NextAttempt).Descending(x => x.IsSending)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Descending(x => x.Created)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Expires), new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }));
        }

        public Task QueryPendingAsync(Func<IRuleEventEntity, Task> callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            var now = clock.GetCurrentInstant();

            return Collection.Find(x => x.NextAttempt < now && !x.IsSending).ForEachAsync(callback, cancellationToken);
        }

        public async Task<IReadOnlyList<IRuleEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20)
        {
            var webhookEventEntities =
                await Collection.Find(x => x.AppId == appId).Skip(skip).Limit(take).SortByDescending(x => x.Created)
                    .ToListAsync();

            return webhookEventEntities;
        }

        public async Task<IRuleEventEntity> FindAsync(Guid id)
        {
            var webhookEventEntity =
                await Collection.Find(x => x.Id == id)
                    .FirstOrDefaultAsync();

            return webhookEventEntity;
        }

        public async Task<int> CountByAppAsync(Guid appId)
        {
            return (int)await Collection.CountAsync(x => x.AppId == appId);
        }

        public Task EnqueueAsync(Guid id, Instant nextAttempt)
        {
            return Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.NextAttempt, nextAttempt));
        }

        public Task EnqueueAsync(RuleJob job, Instant nextAttempt)
        {
            var entity = SimpleMapper.Map(job, new MongoRuleEventEntity { Created = clock.GetCurrentInstant(), NextAttempt = nextAttempt });

            return Collection.InsertOneIfNotExistsAsync(entity);
        }

        public Task MarkSendingAsync(Guid jobId)
        {
            return Collection.UpdateOneAsync(x => x.Id == jobId, Update.Set(x => x.IsSending, true));
        }

        public Task MarkSentAsync(Guid jobId, string dump, RuleResult result, TimeSpan elapsed, Instant? nextAttempt)
        {
            RuleJobResult jobResult;

            if (result != RuleResult.Success && nextAttempt == null)
            {
                jobResult = RuleJobResult.Failed;
            }
            else if (result != RuleResult.Success && nextAttempt.HasValue)
            {
                jobResult = RuleJobResult.Retry;
            }
            else
            {
                jobResult = RuleJobResult.Success;
            }

            return Collection.UpdateOneAsync(x => x.Id == jobId,
                Update.Set(x => x.Result, result)
                      .Set(x => x.LastDump, dump)
                      .Set(x => x.JobResult, jobResult)
                      .Set(x => x.IsSending, false)
                      .Set(x => x.NextAttempt, nextAttempt)
                      .Inc(x => x.NumCalls, 1));
        }
    }
}
