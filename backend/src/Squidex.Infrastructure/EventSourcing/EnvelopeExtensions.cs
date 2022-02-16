// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
            envelope.Headers[CommonHeaders.EventNumber] = JsonValue.Create(value);

            return envelope;
        }

        public static long EventStreamNumber(this EnvelopeHeaders headers)
        {
            return headers.GetLong(CommonHeaders.EventStreamNumber);
        }

        public static Envelope<T> SetEventStreamNumber<T>(this Envelope<T> envelope, long value) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.EventStreamNumber] = JsonValue.Create(value);

            return envelope;
        }

        public static Guid CommitId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.CommitId);
        }

        public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.CommitId] = JsonValue.Create(value);

            return envelope;
        }

        public static DomainId AggregateId(this EnvelopeHeaders headers)
        {
            return DomainId.Create(headers.GetString(CommonHeaders.AggregateId));
        }

        public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, DomainId value) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.AggregateId] = JsonValue.Create(value);

            return envelope;
        }

        public static Guid EventId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.EventId);
        }

        public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.EventId] = JsonValue.Create(value);

            return envelope;
        }

        public static Instant Timestamp(this EnvelopeHeaders headers)
        {
            return headers.GetInstant(CommonHeaders.Timestamp);
        }

        public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.Timestamp] = JsonValue.Create(value);

            return envelope;
        }

        public static bool Restored(this EnvelopeHeaders headers)
        {
            return headers.GetBoolean(CommonHeaders.Restored);
        }

        public static Envelope<T> SetRestored<T>(this Envelope<T> envelope, bool value = true) where T : class, IEvent
        {
            envelope.Headers[CommonHeaders.Restored] = JsonValue.Create(value);

            return envelope;
        }

        public static long GetLong(this EnvelopeHeaders obj, string key)
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

        public static Guid GetGuid(this EnvelopeHeaders obj, string key)
        {
            if (obj.TryGetValue(key, out var v) && v is JsonString s && Guid.TryParse(v.ToString(), out var guid))
            {
                return guid;
            }

            return default;
        }

        public static Instant GetInstant(this EnvelopeHeaders obj, string key)
        {
            if (obj.TryGetValue(key, out var v) && v is JsonString s && InstantPattern.ExtendedIso.Parse(s.ToString()).TryGetValue(default, out var instant))
            {
                return instant;
            }

            return default;
        }

        public static string GetString(this EnvelopeHeaders obj, string key)
        {
            if (obj.TryGetValue(key, out var v))
            {
                return v.ToString();
            }

            return string.Empty;
        }

        public static bool GetBoolean(this EnvelopeHeaders obj, string key)
        {
            if (obj.TryGetValue(key, out var v) && v is JsonBoolean boolean)
            {
                return boolean.Value;
            }

            return false;
        }
    }
}
