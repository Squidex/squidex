﻿// ==========================================================================
//  IAggregate.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Commands
{
    public interface IAggregate
    {
        Guid Id { get; }

        int Version { get; }

        void ApplyEvent(Envelope<IEvent> @event);

        void ClearUncommittedEvents();

        ICollection<Envelope<IEvent>> GetUncomittedEvents();
    }
}