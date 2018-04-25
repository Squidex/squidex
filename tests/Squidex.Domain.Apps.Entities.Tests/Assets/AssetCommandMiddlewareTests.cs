// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddlewareTests : HandlerTestBase<AssetGrain, AssetState>
    {
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly Stream stream = new MemoryStream();
        private readonly ImageInfo image = new ImageInfo(2048, 2048);
        private readonly AssetGrain asset;
        private readonly AssetFile file;
        private readonly AssetCommandMiddleware sut;

        protected override Guid Id
        {
            get { return assetId; }
        }

        public AssetCommandMiddlewareTests()
        {
            file = new AssetFile("my-image.png", "image/png", 1024, () => stream);

            asset = new AssetGrain(Store, A.Dummy<ISemanticLog>());
            asset.OnActivateAsync(Id).Wait();

            A.CallTo(() => grainFactory.GetGrain<IAssetGrain>(Id, null))
                .Returns(asset);

            sut = new AssetCommandMiddleware(grainFactory, assetStore, assetThumbnailGenerator);
        }

        [Fact]
        public async Task Create_should_create_domain_object()
        {
            var context = CreateContextForCommand(new CreateAsset { AssetId = assetId, File = file });

            SetupStore(0, context.ContextId);
            SetupImageInfo();

            await sut.HandleAsync(context);

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

            await ExecuteCreateAsync();

            await sut.HandleAsync(context);

            AssertAssetHasBeenUploaded(1, context.ContextId);
            AssertAssetImageChecked();
        }

        private Task ExecuteCreateAsync()
        {
            return asset.ExecuteAsync(CreateCommand(new CreateAsset { AssetId = Id, File = file }));
        }

        private void SetupStore(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadAsync(commitId.ToString(), stream, CancellationToken.None))
                .Returns(TaskHelper.Done);
            A.CallTo(() => assetStore.CopyAsync(commitId.ToString(), assetId.ToString(), version, null, CancellationToken.None))
                .Returns(TaskHelper.Done);
            A.CallTo(() => assetStore.DeleteAsync(commitId.ToString()))
                .Returns(TaskHelper.Done);
        }

        private void AssertAssetHasBeenUploaded(long version, Guid commitId)
        {
            A.CallTo(() => assetStore.UploadAsync(commitId.ToString(), stream, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.CopyAsync(commitId.ToString(), assetId.ToString(), version, null, CancellationToken.None))
                .MustHaveHappened();
            A.CallTo(() => assetStore.DeleteAsync(commitId.ToString()))
                .MustHaveHappened();
        }

        private void SetupImageInfo()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .Returns(image);
        }

        private void AssertAssetImageChecked()
        {
            A.CallTo(() => assetThumbnailGenerator.GetImageInfoAsync(stream))
                .MustHaveHappened();
        }
    }
}
