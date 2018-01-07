// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
