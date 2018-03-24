// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class SquidexDomainObjectGrain<T> : DomainObjectGrain<T> where T : IDomainState, new()
    {
        protected SquidexDomainObjectGrain(IStore<Guid> store)
            : base(store)
        {
        }

        public override void RaiseEvent(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                @event.SetAppId(appEvent.AppId.Id);
            }

            base.RaiseEvent(@event);
        }
    }
}
