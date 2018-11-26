// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
            return headers[CommonHeaders.EventNumber].ToString();
        }

        public static Envelope<T> SetEventPosition<T>(this Envelope<T> envelope, string value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.EventNumber, value);

            return envelope;
        }

        public static long EventStreamNumber(this EnvelopeHeaders headers)
        {
            return headers.GetInt64(CommonHeaders.EventStreamNumber);
        }

        public static Envelope<T> SetEventStreamNumber<T>(this Envelope<T> envelope, long value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.EventStreamNumber, value);

            return envelope;
        }

        public static Guid CommitId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.CommitId);
        }

        public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.CommitId, value);

            return envelope;
        }

        public static Guid AggregateId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.AggregateId);
        }

        public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.AggregateId, value);

            return envelope;
        }

        public static Guid EventId(this EnvelopeHeaders headers)
        {
            return headers.GetGuid(CommonHeaders.EventId);
        }

        public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.EventId, value);

            return envelope;
        }

        public static Instant Timestamp(this EnvelopeHeaders headers)
        {
            return headers.GetInstant(CommonHeaders.Timestamp);
        }

        public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class
        {
            envelope.Headers.Add(CommonHeaders.Timestamp, value);

            return envelope;
        }

        public static long GetInt64(this JsonObject obj, string key)
        {
            var value = obj[key];

            return value is JsonScalar<double> s ? (long)s.Value : long.Parse(value.ToString(), CultureInfo.InvariantCulture);
        }

        public static Guid GetGuid(this JsonObject obj, string key)
        {
            var value = obj[key];

            return Guid.Parse(value.ToString());
        }

        public static Instant GetInstant(this JsonObject obj, string key)
        {
            var value = obj[key];

            return InstantPattern.General.Parse(value.ToString()).Value;
        }
    }
}
