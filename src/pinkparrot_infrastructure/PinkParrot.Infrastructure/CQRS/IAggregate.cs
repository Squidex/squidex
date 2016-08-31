// ==========================================================================
//  IAggregate.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Infrastructure.CQRS
{
    public interface IAggregate
    {
        Guid Id { get; }

        int Version { get; }

        void ApplyEvent(IEvent @event);

        void ClearUncommittedEvents();

        ICollection<Envelope<IEvent>> GetUncomittedEvents();
    }
}


