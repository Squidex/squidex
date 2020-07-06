// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public class DefaultEventEnricher<TKey> : IEventEnricher<TKey>
    {
        public virtual void Enrich(Envelope<IEvent> @event, TKey key)
        {
            if (key is DomainId domainId)
            {
                @event.SetAggregateId(domainId);
            }
        }
    }
}
