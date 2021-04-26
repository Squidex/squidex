// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
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
