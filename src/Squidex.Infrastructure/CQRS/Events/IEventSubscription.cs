// ==========================================================================
//  IEventSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventSubscription : IDisposable
    {
        Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError = null);
    }
}