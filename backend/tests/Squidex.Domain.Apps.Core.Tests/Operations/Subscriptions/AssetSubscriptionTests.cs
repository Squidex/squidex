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
using Squidex.Infrastructure;

#pragma warning disable CA1859 // Use concrete types when possible for improved performance

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class AssetSubscriptionTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

    [Fact]
    public async Task Should_return_true_for_enriched_asset_event()
    {
        var sut = new AssetSubscription();

        var @event = Enrich(new EnrichedAssetEvent());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_wrong_event()
    {
        var sut = new AssetSubscription();

        var @event = new AppCreated();

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_asset_event()
    {
        var sut = new AssetSubscription();

        var @event = Enrich(new AssetCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_asset_event_with_correct_type()
    {
        var sut = new AssetSubscription { Type = EnrichedAssetEventType.Created };

        var @event = Enrich(new AssetCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_asset_event_with_wrong_type()
    {
        var sut = new AssetSubscription { Type = EnrichedAssetEventType.Deleted };

        var @event = Enrich(new AssetCreated());

        Assert.False(await sut.ShouldHandle(@event));
    }

    private object Enrich(EnrichedAssetEvent source)
    {
        source.AppId = appId;
        return source;
    }

    private object Enrich(AssetEvent source)
    {
        source.AppId = appId;
        return source;
    }
}
