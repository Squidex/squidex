// ==========================================================================
//  IEventConsumer.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================
namespace PinkParrot.Infrastructure.CQRS.Events
{
    public interface IEventConsumer
    {
        void On(Envelope<IEvent> @event);
    }
}
