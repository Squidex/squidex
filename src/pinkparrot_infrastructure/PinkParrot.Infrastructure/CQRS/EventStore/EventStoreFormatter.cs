// ==========================================================================
//  EventStoreFormatter.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using NodaTime;
using PinkParrot.Infrastructure.CQRS.Events;

// ReSharper disable InconsistentNaming

namespace PinkParrot.Infrastructure.CQRS.EventStore
{
    public class EventStoreFormatter
    {
        private readonly JsonSerializerSettings serializerSettings;

        public EventStoreFormatter(JsonSerializerSettings serializerSettings = null)
        {
            this.serializerSettings = serializerSettings ?? new JsonSerializerSettings();
        }

        public Envelope<IEvent> Parse(ResolvedEvent @event)
        {
            var headers = ReadJson<PropertiesBag>(@event.Event.Metadata);

            var eventType = TypeNameRegistry.GetType(@event.Event.EventType);
            var eventData = ReadJson<IEvent>(@event.Event.Data, eventType);

            var envelope = new Envelope<IEvent>(eventData, headers);

            envelope.Headers.Set(CommonHeaders.Timestamp, Instant.FromDateTimeUtc(DateTime.SpecifyKind(@event.Event.Created, DateTimeKind.Utc)));
            envelope.Headers.Set(CommonHeaders.EventNumber, @event.OriginalEventNumber);

            return envelope;
        }

        public EventData ToEventData(Envelope<IEvent> envelope, Guid commitId)
        {
            var eventType = TypeNameRegistry.GetName(envelope.Payload.GetType());

            envelope.Headers.Set(CommonHeaders.CommitId, commitId);

            var headers = WriteJson(envelope.Headers);
            var content = WriteJson(envelope.Payload);

            return new EventData(envelope.Headers.EventId(), eventType, true, content, headers);
        }

        private T ReadJson<T>(byte[] data, Type type = null)
        {
            return (T)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type ?? typeof(T), serializerSettings);
        }

        private byte[] WriteJson(object value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, serializerSettings));
        }
    }
}
