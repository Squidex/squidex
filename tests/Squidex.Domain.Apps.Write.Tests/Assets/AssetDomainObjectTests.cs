// ==========================================================================
//  AssetDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetDomainObjectTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly AssetDomainObject sut;
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly AssetFile file = new AssetFile("my-image.png", "image/png", 1024, () => new MemoryStream());

        public Guid AssetId { get; } = Guid.NewGuid();

        public AssetDomainObjectTests()
        {
            sut = new AssetDomainObject(AssetId, 0);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateAsset { File = file });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateAssetCommand(new CreateAsset { File = file }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { File = file, ImageInfo = image }));

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
        public void Rename_should_throw_exception_if_command_is_not_valid()
        {
            CreateAsset();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Rename(CreateAssetCommand(new RenameAsset()));
            });
        }

        [Fact]
        public void Rename_should_throw_exception_if_new_name_is_equal_to_old_name()
        {
            CreateAsset();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Rename(CreateAssetCommand(new RenameAsset { FileName = file.FileName }));
            });
        }

        [Fact]
        public void Rename_should_create_events()
        {
            CreateAsset();

            sut.Rename(CreateAssetCommand(new RenameAsset { FileName = "my-new-image.png" }));

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
        public void Delete_should_update_properties_create_events()
        {
            CreateAsset();
            UpdateAsset();

            sut.Delete(CreateAssetCommand(new DeleteAsset()));

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetDeleted { DeletedSize = 2048 })
                );
        }

        private void CreateAsset()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { File = file }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void UpdateAsset()
        {
            sut.Update(CreateAssetCommand(new UpdateAsset { File = file }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteAsset()
        {
            sut.Delete(CreateAssetCommand(new DeleteAsset()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        protected T CreateAssetEvent<T>(T @event) where T : AssetEvent
        {
            @event.AssetId = AssetId;

            return CreateEvent(@event);
        }

        protected T CreateAssetCommand<T>(T command) where T : AssetAggregateCommand
        {
            command.AssetId = AssetId;

            return CreateCommand(command);
        }
    }
}
