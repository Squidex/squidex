// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class DefaultAssetFileStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly long assetFileVersion = 21;
        private readonly string fileNameOld;
        private readonly string fileNameNew;
        private readonly DefaultAssetFileStore sut;

        public DefaultAssetFileStoreTests()
        {
            fileNameOld = $"{assetId}_{assetFileVersion}";
            fileNameNew = $"{appId}_{assetId}_{assetFileVersion}";

            sut = new DefaultAssetFileStore(assetStore);
        }

        [Fact]
        public void Should_get_public_url_from_store()
        {
            var url = "http_//squidex.io/assets";

            A.CallTo(() => assetStore.GeneratePublicUrl(fileNameNew))
                .Returns(url);

            var result = sut.GeneratePublicUrl(appId, assetId, assetFileVersion);

            Assert.Equal(url, result);
        }

        [Fact]
        public async Task Should_get_file_size_from_store()
        {
            var size = 1024L;

            A.CallTo(() => assetStore.GetSizeAsync(fileNameNew, default))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion);

            Assert.Equal(size, result);
        }

        [Fact]
        public async Task Should_get_file_size_from_store_with_old_file_name_if_new_name_not_found()
        {
            var size = 1024L;

            A.CallTo(() => assetStore.GetSizeAsync(fileNameOld, default))
                .Throws(new AssetNotFoundException(fileNameOld));

            A.CallTo(() => assetStore.GetSizeAsync(fileNameNew, default))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion);

            Assert.Equal(size, result);
        }

        [Fact]
        public async Task Should_upload_temporary_filet_to_store()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync("Temp", stream);

            A.CallTo(() => assetStore.UploadAsync("Temp", stream, false, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_upload_file_to_store()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync(appId, assetId, assetFileVersion, stream);

            A.CallTo(() => assetStore.UploadAsync(fileNameNew, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_download_file_from_store()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(appId, assetId, assetFileVersion, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileNameNew, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_download_file_from_store_with_old_file_name_if_new_name_not_found()
        {
            var stream = new MemoryStream();

            A.CallTo(() => assetStore.DownloadAsync(fileNameNew, stream, default, CancellationToken.None))
                .Throws(new AssetNotFoundException(fileNameNew));

            await sut.DownloadAsync(appId, assetId, assetFileVersion, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileNameOld, stream, default, CancellationToken.None))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DownloadAsync(fileNameNew, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_copy_file_to_store()
        {
            await sut.CopyAsync("Temp", appId, assetId, assetFileVersion);

            A.CallTo(() => assetStore.CopyAsync("Temp", fileNameNew, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_temporary_file_from_store()
        {
            await sut.DeleteAsync("Temp");

            A.CallTo(() => assetStore.DeleteAsync("Temp"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_file_from_store()
        {
            await sut.DeleteAsync(appId, assetId, assetFileVersion);

            A.CallTo(() => assetStore.DeleteAsync(fileNameNew))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteAsync(fileNameOld))
                .MustHaveHappened();
        }
    }
}
