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
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class AssetSubscriptionTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

    [Fact]
    public async Task Should_return_true_for_enriched_asset_event()
    {
        var sut = WithPermission(new AssetSubscription());

        var @event = Enrich(new EnrichedAssetEvent());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_wrong_event()
    {
        var sut = WithPermission(new AssetSubscription());

        var @event = new AppCreated();

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_asset_event()
    {
        var sut = WithPermission(new AssetSubscription());

        var @event = Enrich(new AssetCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_asset_event_with_correct_type()
    {
        var sut = WithPermission(new AssetSubscription { Type = EnrichedAssetEventType.Created });

        var @event = Enrich(new AssetCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_asset_event_with_wrong_type()
    {
        var sut = WithPermission(new AssetSubscription { Type = EnrichedAssetEventType.Deleted });

        var @event = Enrich(new AssetCreated());

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_asset_event_invalid_permissions()
    {
        var sut = WithPermission(new AssetSubscription(), PermissionIds.AppCommentsCreate);

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

    private AssetSubscription WithPermission(AssetSubscription subscription, string? permissionId = null)
    {
        subscription.AppId = appId.Id;

        permissionId ??= PermissionIds.AppAssetsRead;

        var permission = PermissionIds.ForApp(permissionId, appId.Name);
        var permissions = new PermissionSet(permission);

        subscription.Permissions = permissions;

        return subscription;
    }
}
