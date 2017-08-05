// ==========================================================================
//  AssetCommandHandlerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Tasks;
using Xunit;

// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetCommandHandlerTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly AssetCommandHandler sut;
        private readonly AssetDomainObject asset;
        private readonly Guid assetId = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly AssetFile file;

        public AssetCommandHandlerTests()
        {
            file = new AssetFile("my-image.png", "image/png", 1024, () => stream);

            asset = new AssetDomainObject(assetId, -1);

            sut = new AssetCommandHandler(Handler, assetStore, assetThumbnailGenerator);
        }

        [Fact]
        public async Task Create_should_create_asset()
        {
            var context = CreateContextForCommand(new CreateAsset { AssetId = assetId, File = file });

            SetupStore(0, context.ContextId);
            SetupImageInfo();

            await TestCreate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(assetId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);

            VerifyStore(0, context.ContextId);
            VerifyImageInfo();
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            var context = CreateContextForCommand(new UpdateAsset { AssetId = assetId, File = file });

            SetupStore(1, context.ContextId);
            SetupImageInfo();

            CreateAsset();

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            VerifyStore(1, context.ContextId);
            VerifyImageInfo();
        }

        [Fact]
        public async Task Rename_should_update_domain_object()
        {
            CreateAsset();

            var context = CreateContextForCommand(new RenameAsset { AssetId = assetId, FileName = "my-new-image.png" });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateAsset();

            var command = CreateContextForCommand(new DeleteAsset { AssetId = assetId });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

        private void CreateAsset()
        {
            asset.Create(new CreateAsset { File = file });
        }

        private void SetupImageInfo()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(Task.FromResult(image));
        }

        private void SetupStore(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadTemporaryAsync(commitId.ToString(), stream))
                .Returns(TaskHelper.Done);
            A.CallTo(() => assetStore.CopyTemporaryAsync(commitId.ToString(), assetId.ToString(), version, null))
                .Returns(TaskHelper.Done);
            A.CallTo(() => assetStore.DeleteTemporaryAsync(commitId.ToString()))
                .Returns(TaskHelper.Done);
        }

        private void VerifyImageInfo()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream)).MustHaveHappened();
        }

        private void VerifyStore(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadTemporaryAsync(commitId.ToString(), stream)).MustHaveHappened();
            A.CallTo(() => assetStore.CopyTemporaryAsync(commitId.ToString(), assetId.ToString(), version, null)).MustHaveHappened();
            A.CallTo(() => assetStore.DeleteTemporaryAsync(commitId.ToString())).MustHaveHappened();
        }
    }
}
