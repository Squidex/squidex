// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class DefaultEventDataFormatter : IEventDataFormatter
    {
        private readonly IJsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry;

        public DefaultEventDataFormatter(TypeNameRegistry typeNameRegistry, IJsonSerializer serializer)
        {
            this.typeNameRegistry = typeNameRegistry;

            this.serializer = serializer;
        }

        public Envelope<IEvent>? ParseIfKnown(StoredEvent storedEvent)
        {
            try
            {
                return Parse(storedEvent);
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }

        public Envelope<IEvent> Parse(StoredEvent storedEvent)
        {
            Guard.NotNull(storedEvent, nameof(storedEvent));

            var eventData = storedEvent.Data;

            var payloadType = typeNameRegistry.GetType(eventData.Type);
            var payloadValue = serializer.Deserialize<IEvent>(eventData.Payload, payloadType);

            if (payloadValue is IMigrated<IEvent> migratedEvent)
            {
                payloadValue = migratedEvent.Migrate();

                if (ReferenceEquals(migratedEvent, payloadValue))
                {
                    Debug.WriteLine("Migration should return new event.");
                }
            }

            var envelope = new Envelope<IEvent>(payloadValue, eventData.Headers);

            envelope.SetEventPosition(storedEvent.EventPosition);
            envelope.SetEventStreamNumber(storedEvent.EventStreamNumber);

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
