// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetGrainTests : HandlerTestBase<AssetGrain, AssetState>
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly Guid assetId = Guid.NewGuid();
        private readonly AssetFile file = new AssetFile("my-image.png", "image/png", 1024, () => new MemoryStream());
        private readonly AssetGrain sut;

        protected override Guid Id
        {
            get { return assetId; }
        }

        public AssetGrainTests()
        {
            A.CallTo(() => tagService.NormalizeTagsAsync(AppId, TagGroups.Assets, A<HashSet<string>>.Ignored, A<HashSet<string>>.Ignored))
                .Returns(new HashSet<string>());

            sut = new AssetGrain(Store, tagService, A.Dummy<ISemanticLog>());
            sut.OnActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Command_should_throw_exception_if_rule_is_deleted()
        {
            await ExecuteCreateAsync();
            await ExecuteDeleteAsync();

            await Assert.ThrowsAsync<DomainException>(ExecuteUpdateAsync);
        }

        [Fact]
        public async Task Create_should_create_events()
        {
            var command = new CreateAsset { File = file, ImageInfo = image };

            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            result.ShouldBeEquivalent(new AssetSavedResult(0, 0));

            Assert.Equal(0, sut.Snapshot.FileVersion);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetCreated
                    {
                        IsImage = true,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        FileVersion = 0,
                        MimeType = file.MimeType,
                        PixelWidth = image.PixelWidth,
                        PixelHeight = image.PixelHeight,
                        Tags = new HashSet<string>()
                    })
                );
        }

        [Fact]
        public async Task Update_should_create_events()
        {
            var command = new UpdateAsset { File = file, ImageInfo = image };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            result.ShouldBeEquivalent(new AssetSavedResult(1, 1));

            Assert.Equal(1, sut.Snapshot.FileVersion);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetUpdated
                    {
                        IsImage = true,
                        FileSize = file.FileSize,
                        FileVersion = 1,
                        MimeType = file.MimeType,
                        PixelWidth = image.PixelWidth,
                        PixelHeight = image.PixelHeight
                    })
                );
        }

        [Fact]
        public async Task Rename_should_create_events()
        {
            var command = new RenameAsset { FileName = "my-new-image.png" };

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            Assert.Equal("my-new-image.png", sut.Snapshot.FileName);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetRenamed { FileName = "my-new-image.png" })
                );
        }

        [Fact]
        public async Task Tag_should_create_events()
        {
            var command = new TagAsset();

            await ExecuteCreateAsync();

            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetTagged { Tags = new HashSet<string>() })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_with_total_file_size()
        {
            var command = new DeleteAsset();

            await ExecuteCreateAsync();
            await ExecuteUpdateAsync();

            var result = await sut.ExecuteAsync(CreateAssetCommand(command));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            Assert.True(sut.Snapshot.IsDeleted);

            LastEvents
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetDeleted { DeletedSize = 2048 })
                );
        }

        private Task ExecuteCreateAsync()
        {
            return sut.ExecuteAsync(CreateAssetCommand(new CreateAsset { File = file }));
        }

        private Task ExecuteUpdateAsync()
        {
            return sut.ExecuteAsync(CreateAssetCommand(new UpdateAsset { File = file }));
        }

        private Task ExecuteDeleteAsync()
        {
            return sut.ExecuteAsync(CreateAssetCommand(new DeleteAsset()));
        }

        protected T CreateAssetEvent<T>(T @event) where T : AssetEvent
        {
            @event.AssetId = assetId;

            return CreateEvent(@event);
        }

        protected T CreateAssetCommand<T>(T command) where T : AssetCommand
        {
            command.AssetId = assetId;

            return CreateCommand(command);
        }
    }
}
