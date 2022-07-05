// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class DefaultEventFormatter : IEventFormatter
    {
        private readonly IJsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry;

        public DefaultEventFormatter(TypeNameRegistry typeNameRegistry, IJsonSerializer serializer)
        {
            this.typeNameRegistry = typeNameRegistry;

            this.serializer = serializer;
        }

        public Envelope<IEvent>? ParseIfKnown(StoredEvent storedEvent)
        {
            return ParseCore(storedEvent);
        }

        public Envelope<IEvent> Parse(StoredEvent storedEvent)
        {
            var envelope = ParseCore(storedEvent);

            if (envelope == null)
            {
                throw new TypeNameNotFoundException($"Cannot find event with type name '{storedEvent.Data.Type}'.");
            }

            return envelope;
        }

        private Envelope<IEvent>? ParseCore(StoredEvent storedEvent)
        {
            Guard.NotNull(storedEvent);

            var payloadType = typeNameRegistry.GetTypeOrNull(storedEvent.Data.Type);

            if (payloadType == null)
            {
                return null;
            }

            var payloadValue = serializer.Deserialize<IEvent>(storedEvent.Data.Payload, payloadType);

            if (payloadValue is IMigrated<IEvent> migratedEvent)
            {
                payloadValue = migratedEvent.Migrate();
            }

            var envelope = new Envelope<IEvent>(payloadValue, storedEvent.Data.Headers);

            envelope.SetEventPosition(storedEvent.EventPosition);
            envelope.SetEventStreamNumber(storedEvent.EventStreamNumber);

            return envelope;
        }

        public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
        {
            var payloadValue = envelope.Payload;

            if (migrate && payloadValue is IMigrated<IEvent> migratedEvent)
            {
                payloadValue = migratedEvent.Migrate();
            }

            var payloadType = typeNameRegistry.GetName(payloadValue.GetType());
            var payloadJson = serializer.Serialize(envelope.Payload);

            envelope.SetCommitId(commitId);

            return new EventData(payloadType, envelope.Headers, payloadJson);
        }
    }
}
