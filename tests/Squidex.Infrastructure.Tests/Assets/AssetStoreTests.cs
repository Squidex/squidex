// ==========================================================================
//  AssetStoreTestsBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
            ((IExternalSystem)Sut).Connect();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.DownloadAsync(Id(), 1, "suffix", new MemoryStream()));
        }

        [Fact]
        public Task Should_throw_exception_if_asset_to_copy_is_not_found()
        {
            ((IExternalSystem)Sut).Connect();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => Sut.CopyTemporaryAsync(Id(), Id(), 1, null));
        }

        [Fact]
        public async Task Should_read_and_write_file()
        {
            ((IExternalSystem)Sut).Connect();

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
            ((IExternalSystem)Sut).Connect();

            var tempId = Id();

            var assetId = Id();
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadTemporaryAsync(tempId, assetData);
            await Sut.CopyTemporaryAsync(tempId, assetId, 1, "suffix");

            var readData = new MemoryStream();

            await Sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice()
        {
            ((IExternalSystem)Sut).Connect();

            var tempId = Id();

            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await Sut.UploadTemporaryAsync(tempId, assetData);
            await Sut.DeleteTemporaryAsync(tempId);
            await Sut.DeleteTemporaryAsync(tempId);
        }

        private static string Id()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
