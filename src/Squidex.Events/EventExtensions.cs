// ==========================================================================
//  EventExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using Squidex.Infrastructure.CQRS;

namespace Squidex.Events
{
    public static class EventExtensions
    {
        public static Guid AppId(this EnvelopeHeaders headers)
        {
            return headers["AppId"].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetAppId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set("AppId", value);

            return envelope;
        }

        public static Guid SchemaId(this EnvelopeHeaders headers)
        {
            return headers["SchemaId"].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetSchemaId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set("SchemaId", value);

            return envelope;
        }
    }
}
