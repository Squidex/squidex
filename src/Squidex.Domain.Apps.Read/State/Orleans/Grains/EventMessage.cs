// ==========================================================================
//  EventMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Orleans.Concurrency;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    [Immutable]
    public sealed class EventMessage
    {
        public Envelope<IEvent> Event { get; set; }
    }
}
