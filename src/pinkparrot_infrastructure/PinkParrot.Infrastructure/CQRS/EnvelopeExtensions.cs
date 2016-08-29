// ==========================================================================
//  EnvelopeExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using NodaTime;

namespace PinkParrot.Infrastructure.CQRS
{
    public static class EnvelopeExtensions
    {
        public static int EventNumber<T>(this Envelope<T> envelope) where T : class
        {
            return envelope.Headers[CommonHeaders.EventNumber].ToInt32(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetEventNumber<T>(this Envelope<T> envelope, int value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.EventNumber, value);

            return envelope;
        }

        public static Guid CommitId<T>(this Envelope<T> envelope) where T : class
        {
            return envelope.Headers[CommonHeaders.CommitId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.CommitId, value);

            return envelope;
        }

        public static Guid AggregateId<T>(this Envelope<T> envelope) where T : class
        {
            return envelope.Headers[CommonHeaders.AggregateId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.AggregateId, value);

            return envelope;
        }

        public static Guid EventId<T>(this Envelope<T> envelope) where T : class
        {
            return envelope.Headers[CommonHeaders.EventId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.EventId, value);

            return envelope;
        }

        public static Instant Timestamp<T>(this Envelope<T> envelope) where T : class
        {
            return envelope.Headers[CommonHeaders.Timestamp].ToInstant(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.Timestamp, value);

            return envelope;
        }
    }
}
