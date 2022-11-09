// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure.EventSourcing;

public static class EnvelopeExtensions
{
    public static string EventPosition(this EnvelopeHeaders headers)
    {
        return headers.GetString(CommonHeaders.EventNumber);
    }

    public static Envelope<T> SetEventPosition<T>(this Envelope<T> envelope, string value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.EventNumber] = value;

        return envelope;
    }

    public static long EventStreamNumber(this EnvelopeHeaders headers)
    {
        return headers.GetLong(CommonHeaders.EventStreamNumber);
    }

    public static Envelope<T> SetEventStreamNumber<T>(this Envelope<T> envelope, long value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.EventStreamNumber] = (double)value;

        return envelope;
    }

    public static Guid CommitId(this EnvelopeHeaders headers)
    {
        return headers.GetGuid(CommonHeaders.CommitId);
    }

    public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.CommitId] = value.ToString();

        return envelope;
    }

    public static DomainId AggregateId(this EnvelopeHeaders headers)
    {
        return DomainId.Create(headers.GetString(CommonHeaders.AggregateId));
    }

    public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, DomainId value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.AggregateId] = value;

        return envelope;
    }

    public static Guid EventId(this EnvelopeHeaders headers)
    {
        return headers.GetGuid(CommonHeaders.EventId);
    }

    public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.EventId] = value.ToString();

        return envelope;
    }

    public static Instant Timestamp(this EnvelopeHeaders headers)
    {
        return headers.GetInstant(CommonHeaders.Timestamp);
    }

    public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.Timestamp] = value;

        return envelope;
    }

    public static bool Restored(this EnvelopeHeaders headers)
    {
        return headers.GetBoolean(CommonHeaders.Restored);
    }

    public static Envelope<T> SetRestored<T>(this Envelope<T> envelope, bool value = true) where T : class, IEvent
    {
        envelope.Headers[CommonHeaders.Restored] = value;

        return envelope;
    }

    public static long GetLong(this EnvelopeHeaders obj, string key)
    {
        if (obj.TryGetValue(key, out var found))
        {
            if (found.Value is double d)
            {
                return (long)d;
            }
            else if (found.Value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return (long)result;
            }
        }

        return 0;
    }

    public static Guid GetGuid(this EnvelopeHeaders obj, string key)
    {
        if (obj.TryGetValue(key, out var found) && found.Value is string s && Guid.TryParse(s, out var guid))
        {
            return guid;
        }

        return default;
    }

    public static Instant GetInstant(this EnvelopeHeaders obj, string key)
    {
        if (obj.TryGetValue(key, out var found) && found.Value is string s && InstantPattern.ExtendedIso.Parse(s).TryGetValue(default, out var instant))
        {
            return instant;
        }

        return default;
    }

    public static string GetString(this EnvelopeHeaders obj, string key)
    {
        if (obj.TryGetValue(key, out var found))
        {
            return found.ToString();
        }

        return string.Empty;
    }

    public static bool GetBoolean(this EnvelopeHeaders obj, string key)
    {
        if (obj.TryGetValue(key, out var found) && found.Value is bool b)
        {
            return b;
        }

        return false;
    }
}
