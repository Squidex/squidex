// ==========================================================================
//  EnvelopeExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using NodaTime;

namespace Squidex.Infrastructure.CQRS
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

        public static RefToken Actor(this EnvelopeHeaders headers)
        {
            return RefToken.Parse(headers[CommonHeaders.Actor].ToString());
        }

        public static Envelope<T> SetActor<T>(this Envelope<T> envelope, RefToken value) where T : class
        {
            envelope.Headers.Set(CommonHeaders.Actor, value.ToString());

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
