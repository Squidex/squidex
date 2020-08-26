// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class EventConsumerState
    {
        public bool IsStopped { get; set; }

        public string? Error { get; set; }

        public string? Position { get; set; }

        [JsonConverter(typeof(SeenEventsJsonConverter))]
        public LRUCache<Guid, Guid> RecentlySeenEvents { get; set; }

        public bool IsPaused
        {
            get { return IsStopped && string.IsNullOrWhiteSpace(Error); }
        }

        public bool IsFailed
        {
            get { return IsStopped && !string.IsNullOrWhiteSpace(Error); }
        }

        public EventConsumerState()
            : this(null, null)
        {
        }

        public EventConsumerState(string? position, LRUCache<Guid, Guid>? seenEvents)
        {
            Position = position;

            RecentlySeenEvents = seenEvents ?? new LRUCache<Guid, Guid>(100);
        }

        public bool HasSeen(StoredEvent @event)
        {
            var eventId = @event.Data.Headers.EventId();

            return RecentlySeenEvents.Set(eventId, eventId);
        }

        public EventConsumerState Reset()
        {
            return new EventConsumerState();
        }

        public EventConsumerState Handled(string position)
        {
            return new EventConsumerState(position, RecentlySeenEvents);
        }

        public EventConsumerState Stopped(Exception? ex = null)
        {
            return new EventConsumerState(Position, RecentlySeenEvents) { IsStopped = true, Error = ex?.ToString() };
        }

        public EventConsumerState Started()
        {
            return new EventConsumerState(Position, RecentlySeenEvents) { IsStopped = false };
        }

        public EventConsumerInfo ToInfo(string name)
        {
            return SimpleMapper.Map(this, new EventConsumerInfo { Name = name });
        }
    }
}
