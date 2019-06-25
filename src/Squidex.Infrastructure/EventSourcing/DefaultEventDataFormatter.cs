// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class DefaultEventDataFormatter : IEventDataFormatter
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

        public Envelope<IEvent> Parse(EventData eventData, Func<string, string> stringConverter = null)
        {
            var payloadType = typeNameRegistry.GetType(eventData.Type);
            var payloadObj = serializer.Deserialize<IEvent>(eventData.Payload, payloadType, stringConverter);

            if (payloadObj is IMigrated<IEvent> migratedEvent)
            {
                payloadObj = migratedEvent.Migrate();

                if (ReferenceEquals(migratedEvent, payloadObj))
                {
                    Debug.WriteLine("Migration should return new event.");
                }
            }

            var envelope = new Envelope<IEvent>(payloadObj, eventData.Headers);

            return envelope;
        }

        public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
        {
            var eventPayload = envelope.Payload;

            if (migrate && eventPayload is IMigrated<IEvent> migratedEvent)
            {
                eventPayload = migratedEvent.Migrate();

                if (ReferenceEquals(migratedEvent, eventPayload))
                {
                    Debug.WriteLine("Migration should return new event.");
                }
            }

            var payloadType = typeNameRegistry.GetName(eventPayload.GetType());
            var payloadJson = serializer.Serialize(envelope.Payload);

            envelope.SetCommitId(commitId);

            return new EventData(payloadType, envelope.Headers, payloadJson);
        }
    }
}
