// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventSubscriber
    {
        ValueTask OnEventAsync(IEventSubscription subscription, StoredEvent storedEvent);

        ValueTask OnErrorAsync(IEventSubscription subscription, Exception exception);
    }
}
