// ==========================================================================
//  EventStoreFormatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure.CQRS.Events;

// ReSharper disable InconsistentNaming

namespace Squidex.Infrastructure.CQRS.EventStore
{
    public class EventStoreFormatter
    {
        private readonly JsonSerializerSettings serializerSettings;

        public EventStoreFormatter(JsonSerializerSettings serializerSettings = null)
        {
            this.serializerSettings = serializerSettings ?? new JsonSerializerSettings();
        }

        public Envelope<IEvent> Parse(IReceivedEvent @event)
        {
            var headers = ReadJson<PropertiesBag>(@event.Metadata);

            var eventType = TypeNameRegistry.GetType(@event.EventType);
            var eventData = ReadJson<IEvent>(@event.Payload, eventType);

            var envelope = new Envelope<IEvent>(eventData, headers);

            envelope.Headers.Set(CommonHeaders.Timestamp, Instant.FromDateTimeUtc(DateTime.SpecifyKind(@event.Created, DateTimeKind.Utc)));
            envelope.Headers.Set(CommonHeaders.EventNumber, @event.EventNumber);

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
