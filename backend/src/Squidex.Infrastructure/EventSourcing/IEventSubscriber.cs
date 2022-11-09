// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing;

public delegate IEventSubscription EventSubscriptionSource<T>(IEventSubscriber<T> target);

public interface IEventSubscriber<T>
{
    ValueTask OnNextAsync(IEventSubscription subscription, T @event);

    ValueTask OnErrorAsync(IEventSubscription subscription, Exception exception);
}
