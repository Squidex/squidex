// ==========================================================================
//  AssetUserPictureStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Read.Users
{
    public class AssetUserPictureStoreTests
    {
        private readonly Mock<IAssetStore> assetStore = new Mock<IAssetStore>();
        private readonly AssetUserPictureStore sut;
        private readonly string userId = Guid.NewGuid().ToString();

        public AssetUserPictureStoreTests()
        {
            sut = new AssetUserPictureStore(assetStore.Object);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_picture()
        {
            var stream = new MemoryStream();

            assetStore.Setup(x => x.UploadAsync(userId, 0, "picture", stream))
                .Returns(TaskHelper.Done)
                .Verifiable();

            await sut.UploadAsync(userId, stream);

            assetStore.VerifyAll();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_picture()
        {
            assetStore.Setup(x => x.DownloadAsync(userId, 0, "picture", It.IsAny<Stream>()))
                .Callback<string, long, string, Stream>((id, version, suffix, stream) => stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4 ))
                .Returns(TaskHelper.Done)
                .Verifiable();

            var result = await sut.DownloadAsync(userId);

            Assert.Equal(0, result.Position);
            Assert.Equal(4, result.Length);

            assetStore.VerifyAll();
        }
    }
}
