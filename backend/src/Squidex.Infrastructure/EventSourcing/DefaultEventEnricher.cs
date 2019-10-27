// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventEnricher<TKey> : IEventEnricher<TKey>
    {
        public virtual void Enrich(Envelope<IEvent> @event, TKey id)
        {
            if (id is Guid guid)
            {
                @event.SetAggregateId(guid);
            }
        }
    }
}
