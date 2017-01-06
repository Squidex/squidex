// ==========================================================================
//  IEventPublisher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventPublisher
    {
        void Publish(EventData events);
    }
}
