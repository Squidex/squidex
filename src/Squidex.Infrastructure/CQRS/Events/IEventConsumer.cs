// ==========================================================================
//  IEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventConsumer
    {
        string Name { get; }

        string EventsFilter { get; }

        Task ClearAsync();

        Task On(Envelope<IEvent> @event);
    }
}