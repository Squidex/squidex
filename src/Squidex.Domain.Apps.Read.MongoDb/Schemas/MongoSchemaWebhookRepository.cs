// ==========================================================================
//  MongoSchemaWebhookRepository.cs
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
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

// ReSharper disable SwitchStatementMissingSomeCases

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public partial class MongoSchemaWebhookRepository : MongoRepositoryBase<MongoSchemaWebhookEntity>, ISchemaWebhookRepository, IEventConsumer
    {
        private static readonly List<ShortInfo> EmptyWebhooks = new List<ShortInfo>();
        private Dictionary<Guid, Dictionary<Guid, List<ShortInfo>>> inMemoryWebhooks;
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);

        public sealed class ShortInfo : ISchemaWebhookUrlEntity
        {
            public Guid Id { get; set; }

            public Uri Url { get; set; }

            public string SharedSecret { get; set; }
        }

        public MongoSchemaWebhookRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_SchemaWebhooks";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaWebhookEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.SchemaId));
        }

        public async Task<IReadOnlyList<ISchemaWebhookEntity>> QueryByAppAsync(Guid appId)
        {
            return await Collection.Find(Filter.Eq(x => x.AppId, appId)).ToListAsync();
        }

        public async Task<IReadOnlyList<ISchemaWebhookUrlEntity>> QueryUrlsBySchemaAsync(Guid appId, Guid schemaId)
        {
            await EnsureWebooksLoadedAsync();

            return inMemoryWebhooks.GetOrDefault(appId)?.GetOrDefault(schemaId)?.ToList() ?? EmptyWebhooks;
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
                        var result = new Dictionary<Guid, Dictionary<Guid, List<ShortInfo>>>();

                        var webhooks = await Collection.Find(new BsonDocument()).ToListAsync();

                        foreach (var webhook in webhooks)
                        {
                            var list = result.GetOrAddNew(webhook.AppId).GetOrAddNew(webhook.SchemaId);

                            list.Add(SimpleMapper.Map(webhook, new ShortInfo()));
                        }

                        inMemoryWebhooks = result;
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
