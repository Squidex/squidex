// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class EventMessageEvaluatorTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
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
        var contentSubscriptionId = Guid.NewGuid();
        var contentSubscription = WithPermission(new ContentSubscription(), PermissionIds.AppContentsRead);

        var assetSubscriptionId = Guid.NewGuid();
        var assetSubscription = WithPermission(new AssetSubscription(), PermissionIds.AppAssetsRead);

        sut.SubscriptionAdded(contentSubscriptionId, contentSubscription);
        sut.SubscriptionAdded(assetSubscriptionId, assetSubscription);

        Assert.Equal(new[] { contentSubscriptionId },
            await sut.GetSubscriptionsAsync(Enrich(new ContentCreated())));

        Assert.Equal(new[] { assetSubscriptionId },
            await sut.GetSubscriptionsAsync(Enrich(new AssetCreated())));

        Assert.Empty(
            await sut.GetSubscriptionsAsync(Enrich(new AppCreated())));

        Assert.Empty(
            await sut.GetSubscriptionsAsync(new ContentCreated { AppId = NamedId.Of(DomainId.NewGuid(), "my-app2") }));

        sut.SubscriptionRemoved(contentSubscriptionId, contentSubscription);
        sut.SubscriptionRemoved(assetSubscriptionId, assetSubscription);

        Assert.Empty(
            await sut.GetSubscriptionsAsync(Enrich(new ContentCreated())));

        Assert.Empty(
            await sut.GetSubscriptionsAsync(Enrich(new AssetCreated())));
    }

    private object Enrich(ContentEvent source)
    {
        source.SchemaId = schemaId;
        source.AppId = appId;

        return source;
    }

    private object Enrich(AppEvent source)
    {
        source.Actor = null!;
        source.AppId = appId;

        return source;
    }

    private AppSubscription WithPermission(AppSubscription subscription, string permissionId)
    {
        subscription.AppId = appId.Id;

        var permission = PermissionIds.ForApp(permissionId, appId.Name, schemaId.Name);
        var permissions = new PermissionSet(permission);

        subscription.Permissions = permissions;

        return subscription;
    }
}
