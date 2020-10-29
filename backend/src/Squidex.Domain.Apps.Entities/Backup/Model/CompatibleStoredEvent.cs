// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Backup.Model
{
    public sealed class CompatibleStoredEvent
    {
        [JsonProperty("n")]
        public NewEvent NewEvent;

        [JsonProperty]
        public string StreamName;

        [JsonProperty]
        public string EventPosition;

        [JsonProperty]
        public long EventStreamNumber;

        [JsonProperty]
        public CompatibleEventData Data;

        public static CompatibleStoredEvent V1(StoredEvent stored)
        {
            return new CompatibleStoredEvent
            {
                Data = CompatibleEventData.V1(stored.Data),
                EventPosition = stored.EventPosition,
                EventStreamNumber = stored.EventStreamNumber,
                StreamName = stored.StreamName
            };
        }

        public static CompatibleStoredEvent V2(StoredEvent stored)
        {
            return new CompatibleStoredEvent { NewEvent = NewEvent.V2(stored) };
        }

        public StoredEvent ToStoredEvent()
        {
            if (NewEvent != null)
            {
                return NewEvent.ToStoredEvent();
            }
            else
            {
                var data = Data.ToData();

                return new StoredEvent(StreamName, EventPosition, EventStreamNumber, data);
            }
        }
    }

    public sealed class CompatibleEventData
    {
        [JsonProperty]
        public string Type;

        [JsonProperty]
        public JRaw Payload;

        [JsonProperty]
        public EnvelopeHeaders Metadata;

        public static CompatibleEventData V1(EventData data)
        {
            var payload = new JRaw(data.Payload);

            return new CompatibleEventData { Type = data.Type, Payload = payload, Metadata = data.Headers };
        }

        public EventData ToData()
        {
            return new EventData(Type, Metadata, Payload.ToString(CultureInfo.InvariantCulture));
        }
    }

    public sealed class NewEvent
    {
        [JsonProperty("t")]
        public string EventType;

        [JsonProperty("s")]
        public string StreamName;

        [JsonProperty("p")]
        public string EventPayload;

        [JsonProperty("h")]
        public EnvelopeHeaders EventHeaders;

        public static NewEvent V2(StoredEvent stored)
        {
            return new NewEvent
            {
                EventType = stored.Data.Type,
                EventHeaders = stored.Data.Headers,
                EventPayload = stored.Data.Payload,
                StreamName = stored.StreamName
            };
        }

        public StoredEvent ToStoredEvent()
        {
            var data = new EventData(EventType, EventHeaders, EventPayload);

            return new StoredEvent(StreamName, "0", -1, data);
        }
    }
}
