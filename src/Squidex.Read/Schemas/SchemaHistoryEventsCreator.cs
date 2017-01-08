// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
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
    public class SchemaHistoryEventsCreator : IHistoryEventsCreator
    {
        private readonly ISchemaProvider schemaProvider;

        private static readonly IReadOnlyDictionary<string, string> TextsEN =
            new Dictionary<string, string>
            {
                {
                    TypeNameRegistry.GetName<SchemaCreated>(),
                    "created schema {[Name]}"
                },
                {
                    TypeNameRegistry.GetName<SchemaUpdated>(),
                    "updated schema {[Name]}"
                },
                {
                    TypeNameRegistry.GetName<SchemaPublished>(),
                    "published schema {[Name]}"
                },
                {
                    TypeNameRegistry.GetName<SchemaUnpublished>(),
                    "unpublished schema {[Name]}"
                }
            };

        public SchemaHistoryEventsCreator(ISchemaProvider schemaProvider)
        {
            this.schemaProvider = schemaProvider;
        }

        public IReadOnlyDictionary<string, string> Texts
        {
            get { return TextsEN; }
        }

        protected Task<HistoryEventToStore> On(SchemaCreated @event, EnvelopeHeaders headers)
        {
            var name = @event.Name;

            string channel = $"schemas.{name}";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Name", name));
        }

        protected async Task<HistoryEventToStore> On(SchemaUpdated @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";
            
            return
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Name", name);
        }

        protected async Task<HistoryEventToStore> On(SchemaPublished @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";

            return
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Name", name);
        }

        protected async Task<HistoryEventToStore> On(SchemaUnpublished @event, EnvelopeHeaders headers)
        {
            var name = await FindSchemaNameAsync(headers);

            string channel = $"schemas.{name}";

            return
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Name", name);
        }

        public Task<HistoryEventToStore> CreateEventAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, @event.Headers, (HistoryEventToStore)null);
        }

        private async Task<string> FindSchemaNameAsync(EnvelopeHeaders headers)
        {
            var schema = await schemaProvider.ProviderSchemaByIdAsync(headers.AggregateId());

            return schema.Label ?? schema.Name;
        }
    }
}