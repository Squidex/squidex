// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States
{
    public static class Save
    {
        public static SnapshotInstance<T> Snapshot<T>(T initial)
        {
            return new SnapshotInstance<T>(initial);
        }

        public static EventsInstance Events(int keep = int.MaxValue)
        {
            return new EventsInstance(keep);
        }

        public sealed class SnapshotInstance<T>
        {
            public T Value { get; set; }

            public HandleSnapshot<T> Write { get; }

            public SnapshotInstance(T initial)
            {
                Value = initial;

                Write = (state, _) =>
                {
                    Value = state;
                };
            }
        }

        public sealed class EventsInstance : List<IEvent>
        {
            public HandleEvent Write { get; }

            public EventsInstance(int keep = int.MaxValue)
            {
                Write = @event =>
                {
                    Add(@event.Payload);

                    return Count < keep;
                };
            }
        }
    }
}
