// ==========================================================================
//  MongoSchemaWebhookRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Read.MongoDb.Schemas
{
    public partial class MongoSchemaWebhookRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^schema-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected async Task On(WebhookAdded @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            var theAppId = @event.AppId.Id;
            var theSchemaId = @event.SchemaId.Id;

            var webhook = SimpleMapper.Map(@event, new MongoSchemaWebhookEntity { AppId = theAppId, SchemaId = theSchemaId });

            inMemoryWebhooks.GetOrAddNew(theAppId).GetOrAddNew(theSchemaId).Add(SimpleMapper.Map(@event, new ShortInfo()));

            await Collection.InsertOneAsync(webhook);
        }

        protected async Task On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            inMemoryWebhooks.GetOrDefault(@event.AppId.Id)?.Remove(@event.SchemaId.Id);

            await Collection.DeleteManyAsync(x => x.Id == @event.Id);
        }

        protected async Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            inMemoryWebhooks.GetOrDefault(@event.AppId.Id)?.Remove(@event.SchemaId.Id);

            await Collection.DeleteManyAsync(x => x.SchemaId == @event.SchemaId.Id);
        }
    }
}
