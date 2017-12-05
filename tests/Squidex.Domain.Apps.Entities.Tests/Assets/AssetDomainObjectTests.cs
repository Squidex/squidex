// ==========================================================================
//  AssetDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetDomainObjectTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly Guid assetId = Guid.NewGuid();
        private readonly AssetFile file = new AssetFile("my-image.png", "image/png", 1024, () => new MemoryStream());
        private readonly AssetDomainObject sut = new AssetDomainObject();

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            CreateAsset();

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateAssetCommand(new CreateAsset { File = file }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { File = file, ImageInfo = image }));

            Assert.Equal(0, sut.State.FileVersion);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetCreated
                    {
                        IsImage = true,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        FileVersion = 0,
                        MimeType = file.MimeType,
                        PixelWidth = image.PixelWidth,
                        PixelHeight = image.PixelHeight
                    })
                );
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset { File = file }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_asset_is_deleted()
        {
            CreateAsset();
            DeleteAsset();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset()));
            });
        }

        [Fact]
        public void Update_should_create_events()
        {
            CreateAsset();

            sut.Update(CreateAssetCommand(new UpdateAsset { File = file, ImageInfo = image }));

            Assert.Equal(1, sut.State.FileVersion);

            sut.GetUncomittedEvents()
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
        public void Rename_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Rename(CreateAssetCommand(new RenameAsset { FileName = "new-file.png" }));
            });
        }

        [Fact]
        public void Rename_should_throw_exception_if_asset_is_deleted()
        {
            CreateAsset();
            DeleteAsset();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset()));
            });
        }

        [Fact]
        public void Rename_should_create_events()
        {
            CreateAsset();

            sut.Rename(CreateAssetCommand(new RenameAsset { FileName = "my-new-image.png" }));

            Assert.Equal("my-new-image.png", sut.State.FileName);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetRenamed { FileName = "my-new-image.png" })
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateAssetCommand(new DeleteAsset()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateAsset();
            DeleteAsset();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateAssetCommand(new DeleteAsset()));
            });
        }

        [Fact]
        public void Delete_should_create_events_with_total_file_size()
        {
            CreateAsset();
            UpdateAsset();

            sut.Delete(CreateAssetCommand(new DeleteAsset()));

            Assert.True(sut.State.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetDeleted { DeletedSize = 2048 })
                );
        }

        private void CreateAsset()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { File = file }));
            sut.ClearUncommittedEvents();
        }

        private void UpdateAsset()
        {
            sut.Update(CreateAssetCommand(new UpdateAsset { File = file }));
            sut.ClearUncommittedEvents();
        }

        private void DeleteAsset()
        {
            sut.Delete(CreateAssetCommand(new DeleteAsset()));
            sut.ClearUncommittedEvents();
        }

        protected T CreateAssetEvent<T>(T @event) where T : AssetEvent
        {
            @event.AssetId = assetId;

            return CreateEvent(@event);
        }

        protected T CreateAssetCommand<T>(T command) where T : AssetAggregateCommand
        {
            command.AssetId = assetId;

            return CreateCommand(command);
        }
    }
}
