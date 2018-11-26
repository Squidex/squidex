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

        public Envelope<IEvent> Parse(EventData eventData, bool migrate = true, Func<string, string> stringConverter = null)
        {
            var payloadType = typeNameRegistry.GetType(eventData.Type);
            var payload = serializer.Deserialize<IEvent>(eventData.Payload, payloadType, stringConverter);

            if (migrate && payload is IMigratedEvent migratedEvent)
            {
                payload = migratedEvent.Migrate();
            }

            var envelope = new Envelope<IEvent>(payload, eventData.Headers);

            return envelope;
        }

        public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
        {
            var eventPayload = envelope.Payload;

            if (migrate && eventPayload is IMigratedEvent migratedEvent)
            {
                eventPayload = migratedEvent.Migrate();
            }

            var payloadType = typeNameRegistry.GetName(eventPayload.GetType());
            var payload = serializer.Serialize(envelope.Payload);

            envelope.SetCommitId(commitId);

            return new EventData(payloadType, envelope.Headers, payload);
        }
    }
}
