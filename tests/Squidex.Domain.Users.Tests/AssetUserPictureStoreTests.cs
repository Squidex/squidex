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
using FakeItEasy.Core;
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

            A.CallTo(() => assetStore.UploadAsync(userId, 0, "picture", stream, CancellationToken.None))
                .Returns(TaskHelper.Done);

            await sut.UploadAsync(userId, stream);

            A.CallTo(() => assetStore.UploadAsync(userId, 0, "picture", stream, CancellationToken.None)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_picture()
        {
            A.CallTo(() => assetStore.DownloadAsync(userId, 0, "picture", A<Stream>.Ignored, CancellationToken.None))
                .Invokes(async (IFakeObjectCall call) =>
                {
                    var stream = call.GetArgument<Stream>(3);

                    await stream.WriteAsync(new byte[] { 1, 2, 3, 4 }, 0, 4);
                });

            var result = await sut.DownloadAsync(userId);

            Assert.Equal(0, result.Position);
            Assert.Equal(4, result.Length);

            A.CallTo(() => assetStore.DownloadAsync(userId, 0, "picture", A<Stream>.Ignored, CancellationToken.None)).MustHaveHappened();
        }
    }
}
