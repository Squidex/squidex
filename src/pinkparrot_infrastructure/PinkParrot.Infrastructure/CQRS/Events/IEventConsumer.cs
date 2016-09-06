// ==========================================================================
//  IEventConsumer.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.CQRS.Events
{
    public interface IEventConsumer
    {
        Task On(Envelope<IEvent> @event);
    }
}