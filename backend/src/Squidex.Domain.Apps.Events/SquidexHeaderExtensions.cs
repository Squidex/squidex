// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events
{
    public static class SquidexHeaderExtensions
    {
        public static DomainId AppId(this EnvelopeHeaders headers)
        {
            return headers.GetString(SquidexHeaders.AppId);
        }

        public static Envelope<T> SetAppId<T>(this Envelope<T> envelope, DomainId value) where T : class, IEvent
        {
            envelope.Headers.Add(SquidexHeaders.AppId, value.ToString());

            return envelope;
        }
    }
}
