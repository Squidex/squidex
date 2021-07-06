// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
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
        private readonly AssetOptions options = new AssetOptions();
        private readonly DefaultAssetFileStore sut;

        public DefaultAssetFileStoreTests()
        {
            sut = new DefaultAssetFileStore(assetStore, Options.Create(options));
        }

        public static IEnumerable<object[]> PathCases()
        {
            yield return new object[] { true, "resize=100", "derived/{appId}/{assetId}_{assetFileVersion}_resize=100" };
            yield return new object[] { true, string.Empty, "{appId}/{assetId}_{assetFileVersion}" };
            yield return new object[] { false, "resize=100", "{appId}_{assetId}_{assetFileVersion}_resize=100" };
            yield return new object[] { false, string.Empty, "{appId}_{assetId}_{assetFileVersion}" };
        }

        public static IEnumerable<object?[]> PathCasesOld()
        {
            yield return new object?[] { "resize=100", "{assetId}_{assetFileVersion}_resize=100" };
            yield return new object?[] { string.Empty, "{assetId}_{assetFileVersion}" };
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public void Should_get_public_url_from_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var url = "http_//squidex.io/assets";

            A.CallTo(() => assetStore.GeneratePublicUrl(fullName))
                .Returns(url);

            var result = sut.GeneratePublicUrl(appId, assetId, assetFileVersion, suffix);

            Assert.Equal(url, result);
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_get_file_size_from_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var size = 1024L;

            A.CallTo(() => assetStore.GetSizeAsync(fullName, default))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion, suffix);

            Assert.Equal(size, result);
        }

        [Theory]
        [MemberData(nameof(PathCasesOld))]
        public async Task Should_get_file_size_from_store_with_old_file_name_if_new_name_not_found(string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            var size = 1024L;

            A.CallTo(() => assetStore.GetSizeAsync(A<string>._, default))
                .Throws(new AssetNotFoundException(assetId.ToString()));

            A.CallTo(() => assetStore.GetSizeAsync(fullName, default))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion, suffix);

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

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_upload_file_to_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var stream = new MemoryStream();

            await sut.UploadAsync(appId, assetId, assetFileVersion, suffix, stream);

            A.CallTo(() => assetStore.UploadAsync(fullName, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_download_file_from_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var stream = new MemoryStream();

            await sut.DownloadAsync(appId, assetId, assetFileVersion, suffix, stream);

            A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_download_file_from_store_with_folder_only_if_configured()
        {
            options.FolderPerApp = true;

            var stream = new MemoryStream();

            A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, CancellationToken.None))
                .Throws(new AssetNotFoundException(assetId.ToString())).Once();

            await Assert.ThrowsAsync<AssetNotFoundException>(() => sut.DownloadAsync(appId, assetId, assetFileVersion, null, stream));

            A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, CancellationToken.None))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [MemberData(nameof(PathCasesOld))]
        public async Task Should_download_file_from_store_with_old_file_name_if_new_name_not_found(string suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            var stream = new MemoryStream();

            A.CallTo(() => assetStore.DownloadAsync(A<string>.That.Matches(x => x != fileName), stream, default, CancellationToken.None))
                .Throws(new AssetNotFoundException(assetId.ToString())).Once();

            await sut.DownloadAsync(appId, assetId, assetFileVersion, suffix, stream);

            A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, CancellationToken.None))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_copy_file_to_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            await sut.CopyAsync("Temp", appId, assetId, assetFileVersion, suffix);

            A.CallTo(() => assetStore.CopyAsync("Temp", fullName, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_temporary_file_from_store()
        {
            await sut.DeleteAsync("Temp");

            A.CallTo(() => assetStore.DeleteAsync("Temp"))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_delete_file_from_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            await sut.DeleteAsync(appId, assetId, assetFileVersion, suffix);

            A.CallTo(() => assetStore.DeleteAsync(fullName))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteAsync(A<string>._))
                .MustHaveHappenedANumberOfTimesMatching(x => x == (folderPerApp ? 1 : 2));
        }

        private string GetFullName(string fileName)
        {
            return fileName
                .Replace("{appId}", appId.ToString())
                .Replace("{assetId}", assetId.ToString())
                .Replace("{assetFileVersion}", assetFileVersion.ToString());
        }
    }
}
