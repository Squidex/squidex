// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class BackupContentsTests
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly Rebuilder rebuilder = A.Fake<Rebuilder>();
        private readonly BackupContents sut;

        public BackupContentsTests()
        {
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
            var me = new RefToken(RefTokenType.Subject, "123");

            var appId = Guid.NewGuid();
            var appName = "my-app";

            var assetsUrl = "https://old.squidex.com/api/assets/";
            var assetsUrlApp = "https://old.squidex.com/api/assets/my-app";

            A.CallTo(() => urlGenerator.AssetContentBase())
                .Returns(assetsUrl);

            A.CallTo(() => urlGenerator.AssetContentBase(appName))
                .Returns(assetsUrlApp);

            var writer = A.Fake<IBackupWriter>();

            var context = new BackupContext(appId, new UserMapping(me), writer);

            await sut.BackupEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context);

            A.CallTo(() => writer.WriteJsonAsync(A<string>._,
                A<BackupContents.Urls>.That.Matches(x =>
                    x.Assets == assetsUrl &&
                    x.AssetsApp == assetsUrlApp)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_replace_asset_url_in_content()
        {
            var me = new RefToken(RefTokenType.Subject, "123");

            var appId = Guid.NewGuid();
            var appName = "my-new-app";

            var newAssetsUrl = "https://new.squidex.com/api/assets";
            var newAssetsUrlApp = "https://old.squidex.com/api/assets/my-new-app";

            var oldAssetsUrl = "https://old.squidex.com/api/assets";
            var oldAssetsUrlApp = "https://old.squidex.com/api/assets/my-old-app";

            var reader = A.Fake<IBackupReader>();

            A.CallTo(() => urlGenerator.AssetContentBase())
                .Returns(newAssetsUrl);

            A.CallTo(() => urlGenerator.AssetContentBase(appName))
                .Returns(newAssetsUrlApp);

            A.CallTo(() => reader.ReadJsonAttachmentAsync<BackupContents.Urls>(A<string>._))
                .Returns(new BackupContents.Urls
                {
                    Assets = oldAssetsUrl,
                    AssetsApp = oldAssetsUrlApp
                });

            var data =
                new NamedContentData()
                    .AddField("asset",
                        new ContentFieldData()
                            .AddValue("en", $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")
                            .AddValue("it", $"Asset: {oldAssetsUrl}/my-asset.jpg."))
                    .AddField("assetsInArray",
                        new ContentFieldData()
                            .AddValue("iv",
                                JsonValue.Array(
                                    $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")))
                    .AddField("assetsInObj",
                        new ContentFieldData()
                            .AddValue("iv",
                                JsonValue.Object()
                                    .Add("asset", $"Asset: {oldAssetsUrlApp}/my-asset.jpg.")));

            var updateData =
                new NamedContentData()
                    .AddField("asset",
                        new ContentFieldData()
                            .AddValue("en", $"Asset: {newAssetsUrlApp}/my-asset.jpg.")
                            .AddValue("it", $"Asset: {newAssetsUrl}/my-asset.jpg."))
                    .AddField("assetsInArray",
                        new ContentFieldData()
                            .AddValue("iv",
                                JsonValue.Array(
                                    $"Asset: {newAssetsUrlApp}/my-asset.jpg.")))
                    .AddField("assetsInObj",
                        new ContentFieldData()
                            .AddValue("iv",
                                JsonValue.Object()
                                    .Add("asset", $"Asset: {newAssetsUrlApp}/my-asset.jpg.")));

            var context = new RestoreContext(appId, new UserMapping(me), reader);

            await sut.RestoreEventAsync(Envelope.Create(new AppCreated
            {
                Name = appName
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentUpdated
            {
                Data = data
            }), context);

            Assert.Equal(updateData, data);
        }

        [Fact]
        public async Task Should_restore_states_for_all_contents()
        {
            var me = new RefToken(RefTokenType.Subject, "123");

            var appId = Guid.NewGuid();

            var schemaId1 = NamedId.Of(Guid.NewGuid(), "my-schema1");
            var schemaId2 = NamedId.Of(Guid.NewGuid(), "my-schema2");

            var contentId1 = Guid.NewGuid();
            var contentId2 = Guid.NewGuid();
            var contentId3 = Guid.NewGuid();

            var context = new RestoreContext(appId, new UserMapping(me), A.Fake<IBackupReader>());

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId1,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentCreated
            {
                ContentId = contentId3,
                SchemaId = schemaId2
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new ContentDeleted
            {
                ContentId = contentId2,
                SchemaId = schemaId1
            }), context);

            await sut.RestoreEventAsync(Envelope.Create(new SchemaDeleted
            {
                SchemaId = schemaId2
            }), context);

            var rebuildContents = new HashSet<Guid>();

            var add = new Func<Guid, Task>(id =>
            {
                rebuildContents.Add(id);

                return Task.CompletedTask;
            });

            A.CallTo(() => rebuilder.InsertManyAsync<ContentDomainObject, ContentState>(A<IdSource>._, A<CancellationToken>._))
                .Invokes((IdSource source, CancellationToken _) => source(add));

            await sut.RestoreAsync(context);

            Assert.Equal(new HashSet<Guid>
            {
                contentId1,
                contentId2
            }, rebuildContents);
        }
    }
}
