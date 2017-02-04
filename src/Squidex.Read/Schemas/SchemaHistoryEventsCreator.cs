// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Read.History;
using Squidex.Read.Schemas.Services;

namespace Squidex.Read.Schemas
{
    public class SchemaHistoryEventsCreator : HistoryEventsCreatorBase
    {
        private readonly ISchemaProvider schemaProvider;

        public SchemaHistoryEventsCreator(TypeNameRegistry typeNameRegistry, ISchemaProvider schemaProvider)
            : base(typeNameRegistry)
        {
            Guard.NotNull(schemaProvider, nameof(schemaProvider));

            this.schemaProvider = schemaProvider;

            AddEventMessage<SchemaCreated>(
                "created schema {[Name]}");

            AddEventMessage<SchemaUpdated>(
                "updated schema {[Name]}");

            AddEventMessage<SchemaPublished>(
                "published schema {[Name]}");

            AddEventMessage<SchemaUnpublished>(
                "unpublished schema {[Name]}");
        }

        protected Task<HistoryEventToStore> On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var name = @event.Name;

            string channel = $"schemas.{name}";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", name));
        }

        protected async Task<HistoryEventToStore> On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";
            
            return
                ForEvent(@event, channel)
                    .AddParameter("Name", name);
        }

        protected async Task<HistoryEventToStore> On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";

            return
                ForEvent(@event, channel)
                    .AddParameter("Name", name);
        }

        protected async Task<HistoryEventToStore> On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";

            return
                ForEvent(@event, channel)
                    .AddParameter("Name", name);
        }

        public override Task<HistoryEventToStore> CreateEventAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, @event.Headers, (HistoryEventToStore)null);
        }

        private async Task<string> FindSchemaNameAsync(EnvelopeHeaders headers)
        {
            var schema = await schemaProvider.FindSchemaByIdAsync(headers.AggregateId());

            return schema.Label ?? schema.Name;
        }
    }
}