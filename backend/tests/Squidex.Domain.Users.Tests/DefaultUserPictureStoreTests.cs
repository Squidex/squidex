// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Xunit;

namespace Squidex.Domain.Users
{
    public class DefaultUserPictureStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DefaultUserPictureStore sut;
        private readonly string userId = Guid.NewGuid().ToString();
        private readonly string file;

        public DefaultUserPictureStoreTests()
        {
            file = $"{userId}_0_picture";

            sut = new DefaultUserPictureStore(assetStore);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_picture_using_suffix_for_compatibility()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync(userId, stream);

            A.CallTo(() => assetStore.UploadAsync(file, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_picture_using_suffix_for_compatibility()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(userId, stream);

            A.CallTo(() => assetStore.DownloadAsync(file, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }
    }
}
