// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class SubscriptionPublisherTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly ISubscriptionService subscriptionService = A.Fake<ISubscriptionService>();
    private readonly SubscriptionPublisher sut;

    private sealed class MyEvent : IEvent
    {
    }

    public SubscriptionPublisherTests()
    {
        sut = new SubscriptionPublisher(subscriptionService, Enumerable.Empty<ISubscriptionEventCreator>());
    }

    [Fact]
    public void Should_return_content_and_asset_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("content-", "asset-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        await ((IEventConsumer)sut).ClearAsync();
    }

    [Fact]
    public void Should_return_custom_name_for_name()
    {
        Assert.Equal("Subscriptions", sut.Name);
    }

    [Fact]
    public void Should_not_support_clear()
    {
        Assert.False(sut.CanClear);
    }

    [Fact]
    public void Should_start_from_latest()
    {
        Assert.True(sut.StartLatest);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_handle_events_when_subscription_exists(bool hasSubscriptions)
    {
        var storedEvent =
            new StoredEvent($"asset-{DomainId.Combine(appId, DomainId.NewGuid())}", $"0", 0,
                new EventData("Type", [], "Payload"));

        A.CallTo(() => subscriptionService.HasSubscriptionsAsync($"asset-{appId.Id}", default))
            .Returns(hasSubscriptions);

        Assert.Equal(hasSubscriptions, await sut.HandlesAsync(storedEvent));
    }

    [Fact]
    public async Task Should_not_publish_if_not_app_event()
    {
        var envelope = Envelope.Create(new MyEvent());

        await sut.On(envelope);

        A.CallTo(subscriptionService)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_publish_asset_event()
    {
        var envelope =
            Envelope.Create(
                new AssetCreated { AppId = appId, AssetId = DomainId.NewGuid() });

        await sut.On(envelope);

        A.CallTo(() => subscriptionService.PublishAsync($"asset-{appId.Id}", A<object>._, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_publish_content_event()
    {
        var envelope =
            Envelope.Create(
                new ContentCreated { AppId = appId, ContentId = DomainId.NewGuid() });

        await sut.On(envelope);

        A.CallTo(() => subscriptionService.PublishAsync($"content-{appId.Id}", A<object>._, default))
            .MustHaveHappened();
    }
}
