// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using NodaTime;
using NodaTime.Text;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class EnvelopeExtensions
    {
        public static string EventPosition(this EnvelopeHeaders headers)
        {
            return headers.GetString(CommonHeaders.EventNumber);
        }

        public static Envelope<T> SetEventPosition<T>(this Envelope<T> envelope, string value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.EventNumber, value);

            return envelope;
        }

        public static long EventStreamNumber(this EnvelopeHeaders headers)
        {
            return headers.GetLong(CommonHeaders.EventStreamNumber);
        }

        public static Envelope<T> SetEventStreamNumber<T>(this Envelope<T> envelope, long value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.EventStreamNumber, value);

            return envelope;
        }

        public static Guid CommitId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.CommitId);
        }

        public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.CommitId, value.ToString());

            return envelope;
        }

        public static DomainId AggregateId(this EnvelopeHeaders headers)
        {
            return DomainId.Create(headers.GetString(CommonHeaders.AggregateId));
        }

        public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, DomainId value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.AggregateId, value.ToString());

            return envelope;
        }

        public static Guid EventId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.EventId);
        }

        public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.EventId, value.ToString());

            return envelope;
        }

        public static Instant Timestamp(this EnvelopeHeaders headers)
        {
            return headers.GetInstant(CommonHeaders.Timestamp);
        }

        public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.Timestamp, value.ToString());

            return envelope;
        }

        public static bool Restored(this EnvelopeHeaders headers)
        {
            return headers.GetBoolean(CommonHeaders.Restored);
        }

        public static Envelope<T> SetRestored<T>(this Envelope<T> envelope, bool value = true) where T : class, IEvent
        {
            envelope.Headers.Add(CommonHeaders.Restored, value);

            return envelope;
        }

        public static long GetLong(this JsonObject obj, string key)
        {
            if (obj.TryGetValue(key, out var v))
            {
                if (v is JsonNumber number)
                {
                    return (long)number.Value;
                }
                else if (v.Type == JsonValueType.String && double.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    return (long)result;
                }
            }

            return 0;
        }

        public static Guid GetGuid(this JsonObject obj, string key)
        {
            if (obj.TryGetValue<JsonString>(key, out var v) && Guid.TryParse(v.ToString(), out var guid))
            {
                return guid;
            }

            return default;
        }

        public static Instant GetInstant(this JsonObject obj, string key)
        {
            if (obj.TryGetValue<JsonString>(key, out var v) && InstantPattern.ExtendedIso.Parse(v.ToString()).TryGetValue(default, out var instant))
            {
                return instant;
            }

            return default;
        }

        public static string GetString(this JsonObject obj, string key)
        {
            if (obj.TryGetValue(key, out var v))
            {
                return v.ToString();
            }

            return string.Empty;
        }

        public static bool GetBoolean(this JsonObject obj, string key)
        {
            if (obj.TryGetValue<JsonBoolean>(key, out var v))
            {
                return v.Value;
            }

            return false;
        }
    }
}
