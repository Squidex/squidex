// ==========================================================================
//  IEventSubscriber.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventSubscriber
    {
        Task OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent);

        Task OnErrorAsync(IEventSubscription subscription, Exception exception);
    }
}
