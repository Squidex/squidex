// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents;

public class BackupContentsTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
    private readonly BackupContents sut;

    public BackupContentsTests()
    {
        ct = cts.Token;

        sut = new BackupContents(rebuilder, urlGenerator);
    }

    [Fact]
    public void Should_provide_name()
    {
        Assert.Equal("Contents", sut.Name);
    }

    [Fact]
    public async Task Should_write_asset_urls()
    {
        var me = RefToken.User("123");

        var assetsUrl = "https://old.squidex.com/api/assets/";
        var assetsUrlApp = "https://old.squidex.com/api/assets/my-app";

        A.CallTo(() => urlGenerator.AssetContentBase())
            .Returns(assetsUrl);

        A.CallTo(() => urlGenerator.AssetContentBase(appId.Name))
            .Returns(assetsUrlApp);

        var writer = A.Fake<IBackupWriter>();

        var context = new BackupContext(appId.Id, new UserMapping(me), writer);

        await sut.BackupEventAsync(Envelope.Create(new AppCreated
        {
            Name = appId.Name
        }), context, ct);

        A.CallTo(() => writer.WriteJsonAsync(A<string>._,
            A<BackupContents.Urls>.That.Matches(x =>
                x.Assets == assetsUrl &&
                x.AssetsApp == assetsUrlApp), ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_replace_asset_url_in_content()
    {
        var me = RefToken.User("123");

        var newAssetsUrl = "https://new.squidex.com/api/assets";
        var newAssetsUrlApp = "https://old.squidex.com/api/assets/my-new-app";

        var oldAssetsUrl = "https://old.squidex.com/api/assets";
        var oldAssetsUrlApp = "https://old.squidex.com/api/assets/my-old-app";

        var reader = A.Fake<IBackupReader>();

        A.CallTo(() => urlGenerator.AssetContentBase())
            .Returns(newAssetsUrl);

        A.CallTo(() => urlGenerator.AssetContentBase(appId.Name))
            .Returns(newAssetsUrlApp);

        A.CallTo(() => reader.ReadJsonAsync<BackupContents.Urls>(A<string>._, ct))
            .Returns(new BackupContents.Urls
            {
                Assets = oldAssetsUrl,
                AssetsApp = oldAssetsUrlApp
            });

        var data =
            new ContentData()
                .AddField("asset",
                    new ContentFieldData()
                        .AddLocalized("en", $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")
                        .AddLocalized("it", $"Asset: {oldAssetsUrl}/my-asset.jpg."))
                .AddField("assetsInArray",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            JsonValue.Array(
                                $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")))
                .AddField("assetsInObj",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            new JsonObject()
                                .Add("asset", $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")));

        var updateData =
            new ContentData()
                .AddField("asset",
                    new ContentFieldData()
                        .AddLocalized("en", $"Asset: {newAssetsUrlApp}/my-asset.jpg.")
                        .AddLocalized("it", $"Asset: {newAssetsUrl}/my-asset.jpg."))
                .AddField("assetsInArray",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            JsonValue.Array(
                                $"Asset: {newAssetsUrlApp}/my-asset.jpg.")))
                .AddField("assetsInObj",
                    new ContentFieldData()
                        .AddLocalized("iv",
                            new JsonObject()
                                .Add("asset", $"Asset: {newAssetsUrlApp}/my-asset.jpg.")));

        var context = new RestoreContext(appId.Id, new UserMapping(me), reader, DomainId.NewGuid());

        await sut.RestoreEventAsync(Envelope.Create(new AppCreated
        {
            Name = appId.Name
        }), context, ct);

        await sut.RestoreEventAsync(Envelope.Create(new ContentUpdated
        {
            Data = data
        }), context, ct);

        Assert.Equal(updateData, data);
    }

    [Fact]
    public async Task Should_restore_states_for_all_contents()
    {
        var me = RefToken.User("123");

        var schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
        var schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");

        var contentId1 = DomainId.NewGuid();
        var contentId2 = DomainId.NewGuid();
        var contentId3 = DomainId.NewGuid();

        var context = new RestoreContext(appId.Id, new UserMapping(me), A.Fake<IBackupReader>(), DomainId.NewGuid());

        await sut.RestoreEventAsync(ContentEvent(new ContentCreated
        {
            ContentId = contentId1,
            SchemaId = schemaId1
        }), context, ct);

        await sut.RestoreEventAsync(ContentEvent(new ContentCreated
        {
            ContentId = contentId2,
            SchemaId = schemaId1
        }), context, ct);

        await sut.RestoreEventAsync(ContentEvent(new ContentCreated
        {
            ContentId = contentId3,
            SchemaId = schemaId2
        }), context, ct);

        await sut.RestoreEventAsync(ContentEvent(new ContentDeleted
        {
            ContentId = contentId2,
            SchemaId = schemaId1
        }), context, ct);

        await sut.RestoreEventAsync(Envelope.Create(new SchemaDeleted
        {
            SchemaId = schemaId2
        }), context, ct);

        var rebuildContents = new HashSet<DomainId>();

        A.CallTo(() => rebuilder.InsertManyAsync<ContentDomainObject, ContentDomainObject.State>(A<IEnumerable<DomainId>>._, A<int>._, ct))
            .Invokes(x => rebuildContents.AddRange(x.GetArgument<IEnumerable<DomainId>>(0)!));

        await sut.RestoreAsync(context, ct);

        Assert.Equal(new HashSet<DomainId>
        {
            DomainId.Combine(appId, contentId1),
            DomainId.Combine(appId, contentId2)
        }, rebuildContents);
    }

    private Envelope<ContentEvent> ContentEvent(ContentEvent @event)
    {
        @event.AppId = appId;

        return Envelope.Create(@event).SetAggregateId(DomainId.Combine(appId, @event.ContentId));
    }
}
