// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events
{
    public static class SquidexHeaderExtensions
    {
        public static Guid AppId(this EnvelopeHeaders headers)
        {
            return headers[SquidexHeaders.AppId].ToGuid(CultureInfo.InvariantCulture);
        }

        public static Envelope<T> SetAppId<T>(this Envelope<T> envelope, Guid value) where T : class
        {
            envelope.Headers.Set(SquidexHeaders.AppId, value);

            return envelope;
        }
    }
}
