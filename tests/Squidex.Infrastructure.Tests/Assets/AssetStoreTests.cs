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
        private readonly Lazy<T> sut;

        protected AssetStoreTests()
        {
            sut = new Lazy<T>(CreateStore);
        }

        protected T Sut
        {
            get { return sut.Value; }
        }

        public abstract T CreateStore();

        public abstract void Dispose();

        [Fact]
        public Task Should_throw_exception_if_asset_to_download_is_not_found()
        {
            ((IInitializable)Sut).Initialize();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.DownloadAsync(Id(), 1, "suffix", new MemoryStream()));
        }

        [Fact]
        public Task Should_throw_exception_if_asset_to_copy_is_not_found()
        {
            ((IInitializable)Sut).Initialize();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.CopyAsync(Id(), Id(), 1, null));
        }

        [Fact]
        public async Task Should_read_and_write_file()
        {
            ((IInitializable)Sut).Initialize();

            var assetId = Id();
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadAsync(assetId, 1, "suffix", assetData);

            var readData = new MemoryStream();

            await Sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_commit_temporary_file()
        {
            ((IInitializable)Sut).Initialize();

            var tempId = Id();

            var assetId = Id();
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadAsync(tempId, assetData);
            await Sut.CopyAsync(tempId, assetId, 1, "suffix");

            var readData = new MemoryStream();

            await Sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice_by_name()
        {
            ((IInitializable)Sut).Initialize();

            var tempId = Id();

            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadAsync(tempId, assetData);
            await Sut.DeleteAsync(tempId);
            await Sut.DeleteAsync(tempId);
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice_by_id()
        {
            ((IInitializable)Sut).Initialize();

            var tempId = Id();

            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadAsync(tempId, 0, null, assetData);
            await Sut.DeleteAsync(tempId, 0, null);
            await Sut.DeleteAsync(tempId, 0, null);
        }

        private static string Id()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
