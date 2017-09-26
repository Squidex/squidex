// ==========================================================================
//  MongoWebhookRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Webhooks;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Webhooks
{
    public partial class MongoWebhookRepository : MongoRepositoryBase<MongoWebhookEntity>, IWebhookRepository, IEventConsumer
    {
        private static readonly List<IWebhookEntity> EmptyWebhooks = new List<IWebhookEntity>();
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);
        private Dictionary<Guid, List<IWebhookEntity>> inMemoryWebhooks;

        public MongoWebhookRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_SchemaWebhooks";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoWebhookEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.SchemaIds)));
        }

        public async Task<IReadOnlyList<IWebhookEntity>> QueryByAppAsync(Guid appId)
        {
            var entities =
                await Collection.Find(x => x.AppId == appId)
                    .ToListAsync();

            return entities.OfType<IWebhookEntity>().ToList();
        }

        public async Task<IReadOnlyList<IWebhookEntity>> QueryCachedByAppAsync(Guid appId)
        {
            await EnsureWebooksLoadedAsync();

            return inMemoryWebhooks.GetOrDefault(appId) ?? EmptyWebhooks;
        }

        public async Task TraceSentAsync(Guid webhookId, WebhookResult result, TimeSpan elapsed)
        {
            var webhookEntity =
                await Collection.Find(x => x.Id == webhookId)
                    .FirstOrDefaultAsync();

            if (webhookEntity != null)
            {
                switch (result)
                {
                    case WebhookResult.Success:
                        webhookEntity.TotalSucceeded++;
                        break;
                    case WebhookResult.Failed:
                        webhookEntity.TotalFailed++;
                        break;
                    case WebhookResult.Timeout:
                        webhookEntity.TotalTimedout++;
                        break;
                }

                webhookEntity.TotalRequestTime += (long)elapsed.TotalMilliseconds;

                await Collection.ReplaceOneAsync(x => x.Id == webhookId, webhookEntity);
            }
        }

        private async Task EnsureWebooksLoadedAsync()
        {
            if (inMemoryWebhooks == null)
            {
                try
                {
                    await lockObject.WaitAsync();

                    if (inMemoryWebhooks == null)
                    {
                        inMemoryWebhooks = new Dictionary<Guid, List<IWebhookEntity>>();

                        var webhooks =
                            await Collection.Find(new BsonDocument())
                                .ToListAsync();

                        foreach (var webhook in webhooks)
                        {
                            inMemoryWebhooks.GetOrAddNew(webhook.AppId).Add(webhook);
                        }
                    }
                }
                finally
                {
                    lockObject.Release();
                }
            }
        }
    }
}
