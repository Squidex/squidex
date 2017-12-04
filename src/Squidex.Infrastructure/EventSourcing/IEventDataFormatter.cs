// ==========================================================================
//  IEventDataFormatter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventDataFormatter
    {
        Envelope<IEvent> Parse(EventData eventData, bool migrate = true);

        EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true);
    }
}
