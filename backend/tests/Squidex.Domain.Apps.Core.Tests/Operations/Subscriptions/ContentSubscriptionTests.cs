// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Operations.Subscriptions;

public class ContentSubscriptionTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");

    [Fact]
    public async Task Should_return_true_for_enriched_content_event()
    {
        var sut = WithPermission(new ContentSubscription());

        var @event = Enrich(new EnrichedContentEvent());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_wrong_event()
    {
        var sut = WithPermission(new ContentSubscription());

        var @event = new AppCreated { AppId = appId };

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_content_event()
    {
        var sut = WithPermission(new ContentSubscription());

        var @event = Enrich(new ContentCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_content_event_with_correct_type()
    {
        var sut = WithPermission(new ContentSubscription { Type = EnrichedContentEventType.Created });

        var @event = Enrich(new ContentCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_content_event_with_wrong_type()
    {
        var sut = WithPermission(new ContentSubscription { Type = EnrichedContentEventType.Deleted });

        var @event = Enrich(new ContentCreated());

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_true_for_content_event_with_correct_schema()
    {
        var sut = WithPermission(new ContentSubscription { SchemaName = schemaId.Name });

        var @event = Enrich(new ContentCreated());

        Assert.True(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_content_event_with_wrong_schema()
    {
        var sut = WithPermission(new ContentSubscription { SchemaName = "wrong-schema" });

        var @event = Enrich(new ContentCreated());

        Assert.False(await sut.ShouldHandle(@event));
    }

    [Fact]
    public async Task Should_return_false_for_content_event_invalid_permissions()
    {
        var sut = WithPermission(new ContentSubscription(), PermissionIds.AppCommentsCreate);

        var @event = Enrich(new ContentCreated());

        Assert.False(await sut.ShouldHandle(@event));
    }

    private object Enrich(EnrichedContentEvent source)
    {
        source.AppId = appId;
        source.SchemaId = schemaId;

        return source;
    }

    private object Enrich(ContentEvent source)
    {
        source.AppId = appId;
        source.SchemaId = schemaId;

        return source;
    }

    private ContentSubscription WithPermission(ContentSubscription subscription, string? permissionId = null)
    {
        subscription.AppId = appId.Id;

        permissionId ??= PermissionIds.AppContentsRead;

        var permission = PermissionIds.ForApp(permissionId, appId.Name, schemaId.Name);
        var permissions = new PermissionSet(permission);

        subscription.Permissions = permissions;

        return subscription;
    }
}
