// ==========================================================================
//  AggregateHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Commands.TestHelpers
{
    internal sealed class MyDomainObject : DomainObjectBase<MyDomainObject, object>
    {
        public MyDomainObject RaiseNewEvent(IEvent @event)
        {
            RaiseEvent(@event);

            return this;
        }

        public MyDomainObject RaiseNewEvent(Envelope<IEvent> @event)
        {
            RaiseEvent(@event);

            return this;
        }
    }
}
