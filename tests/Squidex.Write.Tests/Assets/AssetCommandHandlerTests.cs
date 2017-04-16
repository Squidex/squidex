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
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Write.Assets.Commands;
using Squidex.Write.TestHelpers;
using Xunit;
using Moq;
using Squidex.Infrastructure.Tasks;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Write.Assets
{
    public class AssetCommandHandlerTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly Mock<IAssetThumbnailGenerator> assetThumbnailGenerator = new Mock<IAssetThumbnailGenerator>();
        private readonly Mock<IAssetStore> assetStore = new Mock<IAssetStore>();
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

            sut = new AssetCommandHandler(Handler, assetStore.Object, assetThumbnailGenerator.Object);
        }

        [Fact]
        public async Task Create_should_create_asset()
        {
            assetStore.Setup(x => x.UploadAsync(assetId, 0, null, stream)).Returns(TaskHelper.Done).Verifiable();
            assetThumbnailGenerator.Setup(x => x.GetImageInfoAsync(stream)).Returns(Task.FromResult(image)).Verifiable();

            var context = CreateContextForCommand(new CreateAsset { AssetId = assetId, File = file });

            await TestCreate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(assetId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);

            assetStore.VerifyAll();
            assetThumbnailGenerator.VerifyAll();
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            assetStore.Setup(x => x.UploadAsync(assetId, 1, null, stream)).Returns(TaskHelper.Done).Verifiable();
            assetThumbnailGenerator.Setup(x => x.GetImageInfoAsync(stream)).Returns(Task.FromResult(image)).Verifiable();

            CreateAsset();

            var context = CreateContextForCommand(new UpdateAsset { AssetId = assetId, File = file });

            await TestUpdate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            assetStore.VerifyAll();
            assetThumbnailGenerator.VerifyAll();
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
    }
}
