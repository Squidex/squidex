// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class SubscriptionPublisher : IEventConsumer
{
    private readonly ISubscriptionService subscriptionService;
    private readonly IEnumerable<ISubscriptionEventCreator> subscriptionCreators;

    public string Name => "Subscriptions";

    public StreamFilter EventsFilter { get; } = StreamFilter.Prefix("content-", "asset-");

    public bool StartLatest => true;

    public bool CanClear => false;

    public SubscriptionPublisher(ISubscriptionService subscriptionService,
        IEnumerable<ISubscriptionEventCreator> subscriptionCreators)
    {
        this.subscriptionService = subscriptionService;
        this.subscriptionCreators = subscriptionCreators;
    }

    public async ValueTask<bool> HandlesAsync(StoredEvent @event)
    {
        var key = @event.StreamName.Split(DomainId.IdSeparator)[0];

        return await subscriptionService.HasSubscriptionsAsync(key);
    }

    public Task On(Envelope<IEvent> @event)
    {
        if (@event.Payload is AssetEvent assetEvent)
        {
            var wrapper = new EventMessageWrapper(@event.To<AppEvent>(), subscriptionCreators);

            return subscriptionService.PublishAsync($"asset-{assetEvent.AppId.Id}", wrapper);
        }

        if (@event.Payload is ContentEvent contentEvent)
        {
            var wrapper = new EventMessageWrapper(@event.To<AppEvent>(), subscriptionCreators);

            return subscriptionService.PublishAsync($"content-{contentEvent.AppId.Id}", wrapper);
        }

        return Task.CompletedTask;
    }
}
