// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class Envelope
    {
        public static Envelope<TPayload> Create<TPayload>(TPayload payload) where TPayload : class, IEvent
        {
            var eventId = Guid.NewGuid();

            var envelope =
                new Envelope<TPayload>(payload)
                    .SetEventId(eventId)
                    .SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            return envelope;
        }
    }
}
