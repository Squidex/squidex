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
        IEventSubscription Subscribe(Func<StoredEvent, Task> handler);
    }
}