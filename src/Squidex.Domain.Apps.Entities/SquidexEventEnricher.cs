// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class SquidexEventEnricher<T> : DefaultEventEnricher<T>
    {
        public override void Enrich(Envelope<IEvent> @event, T id)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                @event.SetAppId(appEvent.AppId.Id);
            }

            base.Enrich(@event, id);
        }
    }
}
