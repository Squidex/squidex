// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions
{
    public class EventMessageEvaluatorTests
    {
        private readonly EventMessageEvaluator sut = new EventMessageEvaluator();

        [Fact]
        public async Task Should_return_empty_list_when_nothing_registered()
        {
            var assetEvent = new ContentCreated { AppId = NamedId.Of(DomainId.NewGuid(), "my-app2") };

            var subscriptions = await sut.GetSubscriptionsAsync(assetEvent);

            Assert.Empty(subscriptions);
        }

        [Fact]
        public async Task Should_return_matching_subscriptions()
        {
            var appId = NamedId.Of(DomainId.NewGuid(), "my-app");

            var contentSubscriptionId = Guid.NewGuid();
            var contentSubscription = new ContentSubscription { AppId = appId.Id };

            var assetSubscriptionId = Guid.NewGuid();
            var assetSubscription = new AssetSubscription { AppId = appId.Id };

            sut.SubscriptionAdded(contentSubscriptionId, contentSubscription);
            sut.SubscriptionAdded(assetSubscriptionId, assetSubscription);

            Assert.Equal(new[] { contentSubscriptionId },
                await sut.GetSubscriptionsAsync(new ContentCreated { AppId = appId }));

            Assert.Equal(new[] { assetSubscriptionId },
                await sut.GetSubscriptionsAsync(new AssetCreated { AppId = appId }));

            Assert.Empty(
                await sut.GetSubscriptionsAsync(new AppCreated { AppId = appId }));

            Assert.Empty(
                await sut.GetSubscriptionsAsync(new ContentCreated { AppId = NamedId.Of(DomainId.NewGuid(), "my-app2") }));

            sut.SubscriptionRemoved(contentSubscriptionId, contentSubscription);
            sut.SubscriptionRemoved(assetSubscriptionId, assetSubscription);

            Assert.Empty(
                await sut.GetSubscriptionsAsync(new ContentCreated { AppId = appId }));

            Assert.Empty(
                await sut.GetSubscriptionsAsync(new AssetCreated { AppId = appId }));
        }
    }
}
