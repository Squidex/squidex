// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class SquidexDomainObjectBase<T> : DomainObjectBase<T> where T : IDomainState, new()
    {
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
