// ==========================================================================
//  FolderAssetStoreTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class FolderAssetStoreTests : IDisposable
    {
        private readonly FolderAssetStore sut;
        private readonly string testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public FolderAssetStoreTests()
        {
            sut = new FolderAssetStore(testFolder, new Mock<ISemanticLog>().Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }
        }

        [Fact]
        public void Should_create_directory_when_connecting()
        {
            sut.Connect();

            Assert.True(Directory.Exists(testFolder));
        }

        [Fact]
        public void Should_throw_when_creating_directory_failed()
        {
            Assert.Throws<ConfigurationException>(() => new FolderAssetStore(CreateInvalidPath(), new Mock<ISemanticLog>().Object).Connect());
        }

        [Fact]
        public Task Should_throw_exception_if_asset_to_download_is_not_found()
        {
            sut.Connect();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => sut.DownloadAsync(Id(), 1, "suffix", new MemoryStream()));
        }

        [Fact]
        public Task Should_throw_exception_if_asset_to_copy_is_not_found()
        {
            sut.Connect();

            return Assert.ThrowsAsync<AssetNotFoundException>(() => sut.CopyTemporaryAsync(Id(), Id(), 1, null));
        }

        [Fact]
        public async Task Should_read_and_write_file()
        {
            sut.Connect();

            var assetId = Id();
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await sut.UploadAsync(assetId, 1, "suffix", assetData);

            var readData = new MemoryStream();

            await sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_commit_temporary_file()
        {
            sut.Connect();

            var tempId = Id();

            var assetId = Id();
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await sut.UploadTemporaryAsync(tempId, assetData);
            await sut.CopyTemporaryAsync(tempId, assetId, 1, "suffix");

            var readData = new MemoryStream();

            await sut.DownloadAsync(assetId, 1, "suffix", readData);

            Assert.Equal(assetData.ToArray(), readData.ToArray());
        }

        [Fact]
        public async Task Should_ignore_when_deleting_twice()
        {
            sut.Connect();

            var tempId = Id();
            
            var assetData = new MemoryStream(new byte[] { 0x1, 0x2, 0x3, 0x4 });

            await sut.UploadTemporaryAsync(tempId, assetData);
            await sut.DeleteTemporaryAsync(tempId);
            await sut.DeleteTemporaryAsync(tempId);
        }

        private static string Id()
        {
            return Guid.NewGuid().ToString();
        }

        private static string CreateInvalidPath()
        {
            var windir = Environment.GetEnvironmentVariable("windir");

            return !string.IsNullOrWhiteSpace(windir) ? "Z://invalid" : "/proc/invalid";
        }
    }
}
