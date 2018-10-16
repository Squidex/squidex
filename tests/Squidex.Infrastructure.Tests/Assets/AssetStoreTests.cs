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
    public abstract class AssetStoreTests<T> : IDisposable where T : IAssetStore
    {
        private readonly MemoryStream assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });
        private readonly string assetId = Guid.NewGuid().ToString();
        private readonly string tempId = Guid.NewGuid().ToString();
        private readonly Lazy<T> sut;

        protected AssetStoreTests()
        {
            sut = new Lazy<T>(CreateStore);

            ((IInitializable)Sut).InitializeAsync().Wait();
        }

        protected T Sut
        {
            get { return sut.Value; }
        }

        protected string AssetId
        {
            get { return assetId; }
        }

        public abstract T CreateStore();

        public abstract void Dispose();

        [Fact]
        public virtual Task Should_throw_exception_if_asset_to_download_is_not_found()
        {
            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.DownloadAsync(assetId, 1, "suffix", new MemoryStream()));
        }

        [Fact]
        public Task Should_throw_exception_if_asset_to_copy_is_not_found()
        {
            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.CopyAsync(tempId, assetId, 1, null));
        }

        [Fact]
        public async Task Should_read_and_write_file()
        {
            await Sut.UploadAsync(assetId, 1, "suffix", assetData);

            var readData = new MemoryStream();

            await Sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_throw_exception_when_file_to_write_already_exists()
        {
            await Sut.UploadAsync(assetId, 1, "suffix", assetData);

            await Assert.ThrowsAsync<AssetAlreadyExistsException>(() => Sut.UploadAsync(assetId, 1, "suffix", assetData));
        }

        [Fact]
        public virtual async Task Should_read_and_write_temporary_file()
        {
            await Sut.UploadAsync(tempId, assetData);
            await Sut.CopyAsync(tempId, assetId, 1, "suffix");

            var readData = new MemoryStream();

            await Sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_throw_exception_when_temporary_file_to_write_already_exists()
        {
            await Sut.UploadAsync(tempId, assetData);
            await Sut.CopyAsync(tempId, assetId, 1, "suffix");

            await Assert.ThrowsAsync<AssetAlreadyExistsException>(() => Sut.UploadAsync(tempId, assetData));
        }

        [Fact]
        public async Task Should_throw_exception_when_target_file_to_copy_to_already_exists()
        {
            await Sut.UploadAsync(tempId, assetData);
            await Sut.CopyAsync(tempId, assetId, 1, "suffix");

            await Assert.ThrowsAsync<AssetAlreadyExistsException>(() => Sut.CopyAsync(tempId, assetId, 1, "suffix"));
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice_by_name()
        {
            await Sut.UploadAsync(tempId, assetData);
            await Sut.DeleteAsync(tempId);
            await Sut.DeleteAsync(tempId);
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice_by_id()
        {
            await Sut.UploadAsync(tempId, 0, null, assetData);
            await Sut.DeleteAsync(tempId, 0, null);
            await Sut.DeleteAsync(tempId, 0, null);
        }
    }
}
