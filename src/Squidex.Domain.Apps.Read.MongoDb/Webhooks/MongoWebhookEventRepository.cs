// ==========================================================================
//  MongoWebhookEventRepository.cs
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
using Squidex.Domain.Apps.Read.Webhooks;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Webhooks
{
    public sealed class MongoWebhookEventRepository : MongoRepositoryBase<MongoWebhookEventEntity>, IWebhookEventRepository
    {
        private readonly IClock clock;

        public MongoWebhookEventRepository(IMongoDatabase database, IClock clock)
            : base(database)
        {
            Guard.NotNull(clock, nameof(clock));

            this.clock = clock;
        }

        protected override string CollectionName()
        {
            return "WebhookEvents";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoWebhookEventEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.NextAttempt).Descending(x => x.IsSending)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Descending(x => x.Created)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Expires), new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }));
        }

        public Task QueryPendingAsync(Func<IWebhookEventEntity, Task> callback, CancellationToken cancellationToken = default(CancellationToken))
        {
            var now = clock.GetCurrentInstant();

            return Collection.Find(x => x.NextAttempt < now && !x.IsSending).ForEachAsync(callback, cancellationToken);
        }

        public async Task<IReadOnlyList<IWebhookEventEntity>> QueryByAppAsync(Guid appId, int skip = 0, int take = 20)
        {
            var entities = await Collection.Find(x => x.AppId == appId).Skip(skip).Limit(take).SortByDescending(x => x.Created).ToListAsync();

            return entities;
        }

        public async Task<IWebhookEventEntity> FindAsync(Guid id)
        {
            var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

            return entity;
        }

        public async Task<int> CountByAppAsync(Guid appId)
        {
            return (int)await Collection.CountAsync(x => x.AppId == appId);
        }

        public Task EnqueueAsync(Guid id, Instant nextAttempt)
        {
            return Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.NextAttempt, nextAttempt));
        }

        public Task TraceSendingAsync(Guid jobId)
        {
            return Collection.UpdateOneAsync(x => x.Id == jobId, Update.Set(x => x.IsSending, true));
        }

        public Task EnqueueAsync(WebhookJob job, Instant nextAttempt)
        {
            var entity = SimpleMapper.Map(job, new MongoWebhookEventEntity { Created = clock.GetCurrentInstant(), NextAttempt = nextAttempt });

            return Collection.InsertOneIfNotExistsAsync(entity);
        }

        public Task TraceSentAsync(Guid jobId, string dump, WebhookResult result, TimeSpan elapsed, Instant? nextAttempt)
        {
            WebhookJobResult jobResult;

            if (result != WebhookResult.Success && nextAttempt == null)
            {
                jobResult = WebhookJobResult.Failed;
            }
            else if (result != WebhookResult.Success && nextAttempt.HasValue)
            {
                jobResult = WebhookJobResult.Retry;
            }
            else
            {
                jobResult = WebhookJobResult.Success;
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
