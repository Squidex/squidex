// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class RecentEvents
    {
        private readonly HashSet<Guid> eventIds;
        private readonly Queue<(Guid, string)> eventQueue;
        private readonly int capacity;

        public IEnumerable<(Guid, string)> EventQueue => eventQueue;

        public RecentEvents(int capacity = 50)
        {
            this.capacity = capacity;

            eventIds = new HashSet<Guid>(capacity);
            eventQueue = new Queue<(Guid, string)>(capacity);
        }

        public string? FirstPosition()
        {
            if (eventQueue.Count == 0)
            {
                return null;
            }

            return eventQueue.Peek().Item2;
        }

        public bool Add(StoredEvent @event)
        {
            return Add(@event.Data.Headers.EventId(), @event.EventPosition);
        }

        public bool Add(Guid id, string position)
        {
            if (eventIds.Contains(id))
            {
                return false;
            }

            while (eventQueue.Count >= capacity)
            {
                var (storedId, _) = eventQueue.Dequeue();

                eventIds.Remove(storedId);
            }

            eventIds.Add(id);
            eventQueue.Enqueue((id, position));

            return true;
        }

        public static RecentEvents Parse(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new RecentEvents();
            }

            return Parse(input.AsSpan());
        }

        private static RecentEvents Parse(ReadOnlySpan<char> span)
        {
            var result = new RecentEvents();

            while (span.Length > 0)
            {
                var endOfLine = span.IndexOf('\n');

                if (endOfLine < 0)
                {
                    endOfLine = span.Length - 1;
                }

                var line = span[0..endOfLine];

                var separator = line.IndexOf('|');

                if (separator > 0 && separator < line.Length - 1)
                {
                    var guidSpan = line[0..separator];

                    if (Guid.TryParse(guidSpan, out var id))
                    {
                        result.Add(id, line[(separator + 1)..].ToString());
                    }
                }

                span = span[endOfLine..];
                span = span.TrimStart('\n');
            }

            return result;
        }

        public override string? ToString()
        {
            if (eventQueue.Count == 0)
            {
                return null;
            }

            var sb = DefaultPools.StringBuilder.Get();
            try
            {
                foreach (var (id, position) in eventQueue)
                {
                    sb.Append(id);
                    sb.Append('|');
                    sb.Append(position);
                    sb.Append('\n');
                }

                return sb.ToString();
            }
            finally
            {
                DefaultPools.StringBuilder.Return(sb);
            }
        }
    }
}
