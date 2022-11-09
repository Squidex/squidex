// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class EventMessageEvaluator : IMessageEvaluator
{
    private readonly Dictionary<DomainId, Dictionary<Guid, AppSubscription>> subscriptions = new Dictionary<DomainId, Dictionary<Guid, AppSubscription>>();
    private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

    public async ValueTask<IEnumerable<Guid>> GetSubscriptionsAsync(object message)
    {
        if (message is not AppEvent appEvent)
        {
            return Enumerable.Empty<Guid>();
        }

        readerWriterLock.EnterReadLock();
        try
        {
            List<Guid>? result = null;

            if (subscriptions.TryGetValue(appEvent.AppId.Id, out var appSubscriptions))
            {
                foreach (var (id, subscription) in appSubscriptions)
                {
                    if (await subscription.ShouldHandle(appEvent))
                    {
                        result ??= new List<Guid>();
                        result.Add(id);
                    }
                }
            }

            return result ?? Enumerable.Empty<Guid>();
        }
        finally
        {
            readerWriterLock.ExitReadLock();
        }
    }

    public void SubscriptionAdded(Guid id, ISubscription subscription)
    {
        if (subscription is not AppSubscription appSubscription)
        {
            return;
        }

        readerWriterLock.EnterWriteLock();
        try
        {
            subscriptions.GetOrAddNew(appSubscription.AppId)[id] = appSubscription;
        }
        finally
        {
            readerWriterLock.ExitWriteLock();
        }
    }

    public void SubscriptionRemoved(Guid id, ISubscription subscription)
    {
        if (subscription is not AppSubscription appSubscription)
        {
            return;
        }

        readerWriterLock.EnterWriteLock();
        try
        {
            subscriptions.GetOrAddDefault(appSubscription.AppId)?.Remove(id);
        }
        finally
        {
            readerWriterLock.ExitWriteLock();
        }
    }
}
