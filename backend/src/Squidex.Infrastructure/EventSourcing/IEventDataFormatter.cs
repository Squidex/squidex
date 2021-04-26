// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventDataFormatter
    {
        Envelope<IEvent> Parse(StoredEvent storedEvent);

        Envelope<IEvent>? ParseIfKnown(StoredEvent storedEvent);

        EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true);
    }
}
