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
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Repositories; 

namespace Squidex.Read.MongoDb.Schemas
{
    public class MongoSchemaWebhookRepository : MongoRepositoryBase<MongoSchemaWebhookEntity>, ISchemaWebhookRepository, IEventConsumer
    {
        private static readonly List<ISchemaWebhookEntity> EmptyWebhooks = new List<ISchemaWebhookEntity>();
        private Dictionary<Guid, List<MongoSchemaWebhookEntity>> inMemoryWebhooks;
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^schema-"; }
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
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.SchemaId));
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected async Task On(WebhookAdded @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            var webhook = SimpleMapper.Map(@event, new MongoSchemaWebhookEntity { SchemaId = @event.SchemaId.Id });

            inMemoryWebhooks.GetOrAddNew(webhook.SchemaId).Add(webhook);

            await Collection.InsertOneAsync(webhook);
        }

        protected async Task On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            inMemoryWebhooks.GetOrDefault(@event.SchemaId.Id)?.RemoveAll(w => w.Id == @event.Id);

            await Collection.DeleteManyAsync(x => x.Id == @event.Id);
        }

        protected async Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            inMemoryWebhooks.Remove(@event.SchemaId.Id);

            await Collection.DeleteManyAsync(x => x.SchemaId == @event.SchemaId.Id);
        }

        public async Task<IReadOnlyList<ISchemaWebhookEntity>> QueryBySchemaAsync(Guid schemaId)
        {
            await EnsureWebooksLoadedAsync();

            return inMemoryWebhooks.GetOrDefault(schemaId)?.OfType<ISchemaWebhookEntity>()?.ToList() ?? EmptyWebhooks;
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
                        var webhooks = await Collection.Find(new BsonDocument()).ToListAsync();

                        inMemoryWebhooks = webhooks.GroupBy(x => x.SchemaId).ToDictionary(x => x.Key, x => x.ToList());
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
