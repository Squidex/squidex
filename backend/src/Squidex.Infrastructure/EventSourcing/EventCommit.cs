// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.EventSourcing;

public sealed record EventCommit(Guid Id, string StreamName, long Offset, ICollection<EventData> Events)
{
    public static EventCommit Create(Guid id, string streamName, long offset, EventData @event)
    {
        return new EventCommit(id, streamName, offset, new List<EventData> { @event });
    }

    public static EventCommit Create(string streamName, long offset, Envelope<IEvent> envelope, IEventFormatter eventFormatter)
    {
        var id = Guid.NewGuid();

        var eventData = eventFormatter.ToEventData(envelope, id);

        return new EventCommit(id, streamName, offset, new List<EventData> { eventData });
    }
}
