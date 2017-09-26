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
using FakeItEasy;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Tasks;
using Xunit;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

namespace Squidex.Domain.Users
{
    public class AssetUserPictureStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly AssetUserPictureStore sut;
        private readonly string userId = Guid.NewGuid().ToString();

        public AssetUserPictureStoreTests()
        {
            sut = new AssetUserPictureStore(assetStore);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_picture()
        {
            var stream = new MemoryStream();

            A.CallTo(() => assetStore.UploadAsync(userId, 0, "picture", stream))
                .Returns(TaskHelper.Done);

            await sut.UploadAsync(userId, stream);

            A.CallTo(() => assetStore.UploadAsync(userId, 0, "picture", stream)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_picture()
        {
            A.CallTo(() => assetStore.DownloadAsync(userId, 0, "picture", A<Stream>.Ignored))
                .Invokes(async (string id, long version, string suffix, Stream stream) =>
                {
                    await stream.WriteAsync(new byte[] { 1, 2, 3, 4 }, 0, 4);
                });

            var result = await sut.DownloadAsync(userId);

            Assert.Equal(0, result.Position);
            Assert.Equal(4, result.Length);

            A.CallTo(() => assetStore.DownloadAsync(userId, 0, "picture", A<Stream>.Ignored)).MustHaveHappened();
        }
    }
}
