// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing;

public interface IEventFormatter
{
    Envelope<IEvent> Parse(StoredEvent storedEvent);

    Envelope<IEvent>? ParseIfKnown(StoredEvent storedEvent);

    EventData ToEventData(Envelope<IEvent> envelope, Guid commitId, bool migrate = true);
}
