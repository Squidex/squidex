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
        public static int EventNumber(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.EventNumber].ToInt32(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetEventNumber<T>(this Envelope<T> envelope, int value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.EventNumber, value);

            return envelope;
        }

        public static Guid CommitId(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.CommitId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetCommitId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.CommitId, value);

            return envelope;
        }

        public static Guid AggregateId(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.AggregateId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetAggregateId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.AggregateId, value);

            return envelope;
        }

        public static Guid EventId(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.EventId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetEventId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.EventId, value);

            return envelope;
        }

        public static Guid TenantId(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.TenantId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetTenantId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.TenantId, value);

            return envelope;
        }

        public static Instant Timestamp(this EnvelopeHeaders headers)
        {
            return headers[CommonHeaders.Timestamp].ToInstant(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetTimestamp<T>(this Envelope<T> envelope, Instant value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.Timestamp, value);

            return envelope;
        }
    }
}
