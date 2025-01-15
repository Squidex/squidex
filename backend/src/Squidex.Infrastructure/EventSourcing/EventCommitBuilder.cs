// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using Squidex.Events;

namespace Squidex.Infrastructure.EventSourcing;

public static class EventCommitBuilder
{
    public static EventCommit Create(string streamName, long offset, Envelope<IEvent> envelope, IEventFormatter eventFormatter)
    {
        var id = Guid.NewGuid();

        var eventData = eventFormatter.ToEventData(envelope, id);

        return new EventCommit(id, streamName, offset, [eventData]);
    }
}
