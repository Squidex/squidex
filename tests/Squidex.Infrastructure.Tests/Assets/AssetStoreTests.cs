// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public abstract class AssetStoreTests<T> where T : IAssetStore
    {
        private readonly MemoryStream assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });
        private readonly string fileName = Guid.NewGuid().ToString();
        private readonly string sourceFile = Guid.NewGuid().ToString();
        private readonly Lazy<T> sut;

        protected T Sut
        {
            get { return sut.Value; }
        }

        protected string FileName
        {
            get { return fileName; }
        }

        protected AssetStoreTests()
        {
            sut = new Lazy<T>(CreateStore);
        }

        public abstract T CreateStore();

        [Fact]
        public virtual async Task Should_throw_exception_if_asset_to_download_is_not_found()
        {
            await Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.DownloadAsync(fileName, new MemoryStream()));
        }

        [Fact]
        public async Task Should_throw_exception_if_asset_to_copy_is_not_found()
        {
            await Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.CopyAsync(fileName, sourceFile));
        }

        [Fact]
        public async Task Should_throw_exception_if_stream_to_download_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.DownloadAsync("File", null));
        }

        [Fact]
        public async Task Should_throw_exception_if_stream_to_upload_is_null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Sut.UploadAsync("File", null));
        }

        [Fact]
        public async Task Should_throw_exception_if_source_file_name_to_copy_is_empty()
        {
            await CheckEmpty(v => Sut.CopyAsync(v, "Target"));
        }

        [Fact]
        public async Task Should_throw_exception_if_target_file_name_to_copy_is_empty()
        {
            await CheckEmpty(v => Sut.CopyAsync("Source", v));
        }

        [Fact]
        public async Task Should_throw_exception_if_file_name_to_delete_is_empty()
        {
            await CheckEmpty(v => Sut.DeleteAsync(v));
        }

        [Fact]
        public async Task Should_throw_exception_if_file_name_to_download_is_empty()
        {
            await CheckEmpty(v => Sut.DownloadAsync(v, new MemoryStream()));
        }

        [Fact]
        public async Task Should_throw_exception_if_file_name_to_upload_is_empty()
        {
            await CheckEmpty(v => Sut.UploadAsync(v, new MemoryStream()));
        }

        [Fact]
        public async Task Should_write_and_read_file()
        {
            await Sut.UploadAsync(fileName, assetData);

            var readData = new MemoryStream();

            await Sut.DownloadAsync(fileName, readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_write_and_read_file_and_overwrite_non_existing()
        {
            await Sut.UploadAsync(fileName, assetData, true);

            var readData = new MemoryStream();

            await Sut.DownloadAsync(fileName, readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_write_and_read_overriding_file()
        {
            var oldData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5 });

            await Sut.UploadAsync(fileName, oldData);
            await Sut.UploadAsync(fileName, assetData, true);

            var readData = new MemoryStream();

            await Sut.DownloadAsync(fileName, readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_throw_exception_when_file_to_write_already_exists()
        {
            await Sut.UploadAsync(fileName, assetData);

            await Assert.ThrowsAsync<AssetAlreadyExistsException>(() => Sut.UploadAsync(fileName, assetData));
        }

        [Fact]
        public async Task Should_throw_exception_when_target_file_to_copy_to_already_exists()
        {
            await Sut.UploadAsync(sourceFile, assetData);
            await Sut.CopyAsync(sourceFile, fileName);

            await Assert.ThrowsAsync<AssetAlreadyExistsException>(() => Sut.CopyAsync(sourceFile, fileName));
        }

        [Fact]
        public async Task Should_ignore_when_deleting_not_existing_file()
        {
            await Sut.UploadAsync(sourceFile, assetData);
            await Sut.DeleteAsync(sourceFile);
            await Sut.DeleteAsync(sourceFile);
        }

        private async Task CheckEmpty(Func<string, Task> action)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => action(null));
            await Assert.ThrowsAsync<ArgumentException>(() => action(string.Empty));
            await Assert.ThrowsAsync<ArgumentException>(() => action(" "));
        }
    }
}
