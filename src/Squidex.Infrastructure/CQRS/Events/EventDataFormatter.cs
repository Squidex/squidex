// ==========================================================================
//  EventDataFormatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EventDataFormatter
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly TypeNameRegistry typeNameRegistry;

        public EventDataFormatter(TypeNameRegistry typeNameRegistry, JsonSerializerSettings serializerSettings = null)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.typeNameRegistry = typeNameRegistry;

            this.serializerSettings = serializerSettings ?? new JsonSerializerSettings();
        }

        public virtual Envelope<IEvent> Parse(EventData eventData, bool migrate = true)
        {
            var headers = ReadJson<PropertiesBag>(eventData.Metadata);

            var eventType = typeNameRegistry.GetType(eventData.Type);
            var eventPayload = ReadJson<IEvent>(eventData.Payload, eventType);

            if (migrate && eventPayload is IMigratedEvent migratedEvent)
            {
                eventPayload = migratedEvent.Migrate();
            }

            var envelope = new Envelope<IEvent>(eventPayload, headers);

            return envelope;
        }

        public virtual EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true)
        {
            var eventPayload = envelope.Payload;

            if (migrate && eventPayload is IMigratedEvent migratedEvent)
            {
                eventPayload = migratedEvent.Migrate();
            }

            var eventType = typeNameRegistry.GetName(eventPayload.GetType());

            envelope.SetCommitId(commitId);

            var headers = WriteJson(envelope.Headers);
            var content = WriteJson(envelope.Payload);

            return new EventData { EventId = envelope.Headers.EventId(), Type = eventType, Payload = content, Metadata = headers };
        }

        private T ReadJson<T>(string data, Type type = null)
        {
            return (T)JsonConvert.DeserializeObject(data, type ?? typeof(T), serializerSettings);
        }

        private string WriteJson(object value)
        {
            return JsonConvert.SerializeObject(value, serializerSettings);
        }
    }
}
