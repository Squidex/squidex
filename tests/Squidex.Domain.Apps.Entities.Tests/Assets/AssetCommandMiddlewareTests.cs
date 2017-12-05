// ==========================================================================
//  AssetCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetDomainObject>
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly AssetDomainObject asset = new AssetDomainObject();
        private readonly AssetFile file;
        private readonly AssetCommandMiddleware sut;

        public AssetCommandMiddlewareTests()
        {
            file = new AssetFile("my-image.png", "image/png", 1024, () => stream);

            sut = new AssetCommandMiddleware(Handler, assetStore, assetThumbnailGenerator);
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var context = CreateContextForCommand(new CreateAsset { AssetId = assetId, File = file });

            SetupStore(0, context.ContextId);
            SetupImageInfo();

            await TestCreate(asset, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(assetId, context.Result<EntityCreatedResult<Guid>>().IdOrValue);

            AssertAssetHasBeenUploaded(0, context.ContextId);
            AssertAssetImageChecked();
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

            AssertAssetHasBeenUploaded(1, context.ContextId);
            AssertAssetImageChecked();
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
            asset.Create(CreateCommand(new CreateAsset { File = file }));
        }

        private void SetupImageInfo()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(image);
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

        private void AssertAssetImageChecked()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream)).MustHaveHappened();
        }

        private void AssertAssetHasBeenUploaded(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadTemporaryAsync(commitId.ToString(), stream)).MustHaveHappened();
            A.CallTo(() => assetStore.CopyTemporaryAsync(commitId.ToString(), assetId.ToString(), version, null)).MustHaveHappened();
            A.CallTo(() => assetStore.DeleteTemporaryAsync(commitId.ToString())).MustHaveHappened();
        }
    }
}
