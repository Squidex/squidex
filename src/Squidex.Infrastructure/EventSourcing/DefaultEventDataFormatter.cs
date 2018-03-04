// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventDataFormatter : IEventDataFormatter
    {
        private readonly JsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry;

        public DefaultEventDataFormatter(TypeNameRegistry typeNameRegistry, JsonSerializer serializer = null)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            this.serializer = serializer ?? JsonSerializer.CreateDefault();
        }

        public Envelope<IEvent> Parse(EventData eventData, bool migrate = true)
        {
            var eventType = typeNameRegistry.GetType(eventData.Type);

            var headers = eventData.Metadata.ToObject<EnvelopeHeaders>(serializer);
            var content = eventData.Payload.ToObject(eventType, serializer) as IEvent;

            if (migrate && content is IMigratedEvent migratedEvent)
            {
                content = migratedEvent.Migrate();
            }

            var envelope = new Envelope<IEvent>(content, headers);

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

            var headers = JToken.FromObject(envelope.Headers, serializer);
            var content = JToken.FromObject(envelope.Payload, serializer);

            return new EventData { Type = eventType, Payload = content, Metadata = headers };
        }
    }
}
