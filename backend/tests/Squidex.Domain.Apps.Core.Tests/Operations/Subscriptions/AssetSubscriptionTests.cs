// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions
{
    public class AssetSubscriptionTests
    {
        [Fact]
        public async Task Should_return_true_for_enriched_asset_event()
        {
            var sut = new AssetSubscription();

            Assert.True(await sut.ShouldHandle(new EnrichedAssetEvent()));
        }

        [Fact]
        public async Task Should_return_false_for_wrong_event()
        {
            var sut = new AssetSubscription();

            Assert.False(await sut.ShouldHandle(new AppCreated()));
        }

        [Fact]
        public async Task Should_return_true_for_asset_event()
        {
            var sut = new AssetSubscription();

            Assert.True(await sut.ShouldHandle(new AssetCreated()));
        }

        [Fact]
        public async Task Should_return_true_for_asset_event_with_correct_type()
        {
            var sut = new AssetSubscription { Type = EnrichedAssetEventType.Created };

            Assert.True(await sut.ShouldHandle(new AssetCreated()));
        }

        [Fact]
        public async Task Should_return_false_for_asset_event_with_wrong_type()
        {
            var sut = new AssetSubscription { Type = EnrichedAssetEventType.Deleted };

            Assert.False(await sut.ShouldHandle(new AssetCreated()));
        }
    }
}
