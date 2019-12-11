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
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class DefaultAssetFileStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly Guid assetId = Guid.NewGuid();
        private readonly long assetFileVersion = 21;
        private readonly string fileName;
        private readonly DefaultAssetFileStore sut;

        public DefaultAssetFileStoreTests()
        {
            fileName = $"{assetId}_{assetFileVersion}";

            sut = new DefaultAssetFileStore(assetStore);
        }

        [Fact]
        public void Should_invoke_asset_store_to_generate_public_url()
        {
            var url = "http_//squidex.io/assets";

            A.CallTo(() => assetStore.GeneratePublicUrl(fileName))
                .Returns(url);

            var result = sut.GeneratePublicUrl(assetId, assetFileVersion);

            Assert.Equal(url, result);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_temporary_upload_file()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync("Temp", stream);

            A.CallTo(() => assetStore.UploadAsync("Temp", stream, false, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_file()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync(assetId, assetFileVersion, stream);

            A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_file()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(assetId, assetFileVersion, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileName, stream, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_copy_from_temporary_file()
        {
            await sut.CopyAsync("Temp", assetId, assetFileVersion);

            A.CallTo(() => assetStore.CopyAsync("Temp", fileName, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_delete_temporary_file()
        {
            await sut.DeleteAsync("Temp");

            A.CallTo(() => assetStore.DeleteAsync("Temp"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_delete_file()
        {
            await sut.DeleteAsync(assetId, assetFileVersion);

            A.CallTo(() => assetStore.DeleteAsync(fileName))
                .MustHaveHappened();
        }
    }
}
