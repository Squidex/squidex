// ==========================================================================
//  AssetDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Write.Assets.Commands;
using Squidex.Write.TestHelpers;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Assets
{
    public class AssetDomainObjectTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly AssetDomainObject sut;
        private readonly string fileName = "my-image.png";
        private readonly string mimeType = "image/png";
        private readonly long fileSize = 1024;

        public Guid AssetId { get; } = Guid.NewGuid();

        public AssetDomainObjectTests()
        {
            sut = new AssetDomainObject(AssetId, 0);
        }

        [Fact]
        public void Create_should_throw_if_created()
        {
            sut.Create(new CreateAsset { FileName = fileName, FileSize = fileSize, MimeType = mimeType });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateAssetCommand(new CreateAsset { FileName = fileName, FileSize = fileSize, MimeType = mimeType }));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { FileName = fileName, FileSize = fileSize, MimeType = mimeType }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetCreated { FileName = fileName, FileSize = fileSize, MimeType = mimeType })
                );
        }

        [Fact]
        public void Update_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset { FileSize = fileSize, MimeType = mimeType }));
            });
        }

        [Fact]
        public void Update_should_throw_if_asset_is_deleted()
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

            sut.Update(CreateAssetCommand(new UpdateAsset { FileSize = fileSize, MimeType = mimeType }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetUpdated { FileSize = fileSize, MimeType = mimeType })
                );
        }

        [Fact]
        public void Rename_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset { FileSize = fileSize, MimeType = mimeType }));
            });
        }

        [Fact]
        public void Rename_should_throw_if_asset_is_deleted()
        {
            CreateAsset();
            DeleteAsset();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateAssetCommand(new UpdateAsset()));
            });
        }

        [Fact]
        public void Rename_should_throw_if_command_is_not_valid()
        {
            CreateAsset();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Rename(CreateAssetCommand(new RenameAsset()));
            });
        }

        [Fact]
        public void Rename_should_throw_if_new_name_is_equal_to_old_name()
        {
            CreateAsset();
            
            Assert.Throws<ValidationException>(() =>
            {
                sut.Rename(CreateAssetCommand(new RenameAsset { FileName = fileName }));
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
        public void Delete_should_throw_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateAssetCommand(new DeleteAsset()));
            });
        }

        [Fact]
        public void Delete_should_throw_if_already_deleted()
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

            sut.Delete(CreateAssetCommand(new DeleteAsset()));

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateAssetEvent(new AssetDeleted())
                );
        }

        private void CreateAsset()
        {
            sut.Create(CreateAssetCommand(new CreateAsset { FileName = fileName, FileSize = fileSize, MimeType = mimeType }));

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
