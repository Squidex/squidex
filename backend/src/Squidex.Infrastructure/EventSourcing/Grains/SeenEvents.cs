// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class SeenEvents : Dictionary<Guid, Instant>
    {
        private static readonly Duration LastSeenDuration = Duration.FromSeconds(2);

        public bool HasSeen(StoredEvent @event)
        {
            var eventTime = @event.Data.Headers.Timestamp();
            var eventId = @event.Data.Headers.EventId();

            CleanupOldEntries(eventTime);

            if (!ContainsKey(eventId))
            {
                Add(eventId, eventTime);

                return false;
            }
            else
            {
                this[eventId] = eventTime;

                return true;
            }
        }

        private void CleanupOldEntries(Instant now)
        {
            HashSet<Guid>? idsToRemove = null;

            foreach (var (id, timestamp) in this)
            {
                var elapsed = now - timestamp;

                if (elapsed > LastSeenDuration)
                {
                    idsToRemove ??= new HashSet<Guid>();
                    idsToRemove.Add(id);
                }
            }

            if (idsToRemove != null)
            {
                foreach (var id in idsToRemove)
                {
                    Remove(id);
                }
            }
        }
    }
}
