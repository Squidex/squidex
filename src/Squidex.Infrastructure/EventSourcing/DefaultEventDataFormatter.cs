// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventDataFormatter : IEventDataFormatter
    {
        private readonly IJsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry;

        public DefaultEventDataFormatter(TypeNameRegistry typeNameRegistry, IJsonSerializer serializer)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));
            Guard.NotNull(serializer, nameof(serializer));

            this.typeNameRegistry = typeNameRegistry;

            this.serializer = serializer;
        }

        public Envelope<IEvent> Parse(EventData eventData, bool migrate = true)
        {
            var eventType = typeNameRegistry.GetType(eventData.Type);

            var eventHeaders = serializer.Deserialize<EnvelopeHeaders>(eventData.Metadata);
            var eventContent = serializer.Deserialize<IEvent>(eventData.Payload, eventType);

            if (migrate && eventContent is IMigratedEvent migratedEvent)
            {
                eventContent = migratedEvent.Migrate();
            }

            var envelope = new Envelope<IEvent>(eventContent, eventHeaders);

            return envelope;
        }

        public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
        {
            var eventPayload = envelope.Payload;

            if (migrate && eventPayload is IMigratedEvent migratedEvent)
            {
                eventPayload = migratedEvent.Migrate();
            }

            var eventType = typeNameRegistry.GetName(eventPayload.GetType());

            envelope.SetCommitId(commitId);

            var eventHeaders = serializer.Serialize(envelope.Headers);
            var eventContent = serializer.Serialize(envelope.Payload);

            return new EventData { Type = eventType, Payload = eventContent, Metadata = eventHeaders };
        }
    }
}
