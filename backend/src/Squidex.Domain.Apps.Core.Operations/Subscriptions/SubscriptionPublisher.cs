// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class SubscriptionPublisher : IEventConsumer
{
    private readonly ISubscriptionService subscriptionService;
    private readonly IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators;

    public string Name
    {
        get => "Subscriptions";
    }

    public string EventsFilter
    {
        get => "^(content-|asset-)";
    }

    public bool StartLatest
    {
        get => true;
    }

    public bool CanClear
    {
        get => false;
    }

    public SubscriptionPublisher(ISubscriptionService subscriptionService, IEnumerable<ISubscriptionEventCreator> subscriptionEventCreators)
    {
        this.subscriptionService = subscriptionService;
        this.subscriptionEventCreators = subscriptionEventCreators;
    }

    public bool Handles(StoredEvent @event)
    {
        return subscriptionService.HasSubscriptions;
    }

    public Task On(Envelope<IEvent> @event)
    {
        if (@event.Payload is not AppEvent)
        {
            return Task.CompletedTask;
        }

        var wrapper = new EventMessageWrapper(@event.To<AppEvent>(), subscriptionEventCreators);

        return subscriptionService.PublishAsync(wrapper);
    }
}
