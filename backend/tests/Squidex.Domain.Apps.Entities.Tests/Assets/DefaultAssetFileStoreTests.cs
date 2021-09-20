// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class DefaultAssetFileStoreTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly long assetFileVersion = 21;
        private readonly AssetOptions options = new AssetOptions();
        private readonly DefaultAssetFileStore sut;

        public DefaultAssetFileStoreTests()
        {
            ct = cts.Token;

            sut = new DefaultAssetFileStore(assetStore, assetRepository, Options.Create(options));
        }

        public static IEnumerable<object[]> PathCases()
        {
            yield return new object[] { true, "resize=100", "{appId}/{assetId}_{assetFileVersion}_resize=100" };
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

            A.CallTo(() => assetStore.GetSizeAsync(fullName, ct))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion, suffix, ct);

            Assert.Equal(size, result);
        }

        [Theory]
        [MemberData(nameof(PathCasesOld))]
        public async Task Should_get_file_size_from_store_with_old_file_name_if_new_name_not_found(string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            var size = 1024L;

            A.CallTo(() => assetStore.GetSizeAsync(A<string>._, ct))
                .Throws(new AssetNotFoundException(assetId.ToString()));

            A.CallTo(() => assetStore.GetSizeAsync(fullName, ct))
                .Returns(size);

            var result = await sut.GetFileSizeAsync(appId, assetId, assetFileVersion, suffix, ct);

            Assert.Equal(size, result);
        }

        [Fact]
        public async Task Should_upload_temporary_filet_to_store()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync("Temp", stream, ct);

            A.CallTo(() => assetStore.UploadAsync("Temp", stream, false, ct))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_upload_file_to_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var stream = new MemoryStream();

            await sut.UploadAsync(appId, assetId, assetFileVersion, suffix, stream, true, ct);

            A.CallTo(() => assetStore.UploadAsync(fullName, stream, true, ct))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_download_file_from_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            var stream = new MemoryStream();

            await sut.DownloadAsync(appId, assetId, assetFileVersion, suffix, stream, default, ct);

            A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_download_file_from_store_with_folder_only_if_configured()
        {
            options.FolderPerApp = true;

            var stream = new MemoryStream();

            A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, ct))
                .Throws(new AssetNotFoundException(assetId.ToString())).Once();

            await Assert.ThrowsAsync<AssetNotFoundException>(() => sut.DownloadAsync(appId, assetId, assetFileVersion, null, stream, default, ct));

            A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, ct))
                .MustHaveHappenedOnceExactly();
        }

        [Theory]
        [MemberData(nameof(PathCasesOld))]
        public async Task Should_download_file_from_store_with_old_file_name_if_new_name_not_found(string suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            var stream = new MemoryStream();

            A.CallTo(() => assetStore.DownloadAsync(A<string>.That.Matches(x => x != fileName), stream, default, ct))
                .Throws(new AssetNotFoundException(assetId.ToString())).Once();

            await sut.DownloadAsync(appId, assetId, assetFileVersion, suffix, stream, default, ct);

            A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, ct))
                .MustHaveHappened();
        }

        [Theory]
        [MemberData(nameof(PathCases))]
        public async Task Should_copy_file_to_store(bool folderPerApp, string? suffix, string fileName)
        {
            var fullName = GetFullName(fileName);

            options.FolderPerApp = folderPerApp;

            await sut.CopyAsync("Temp", appId, assetId, assetFileVersion, suffix, ct);

            A.CallTo(() => assetStore.CopyAsync("Temp", fullName, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_temporary_file_from_store()
        {
            await sut.DeleteAsync("Temp", ct);

            A.CallTo(() => assetStore.DeleteAsync("Temp", ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_file_from_store()
        {
            await sut.DeleteAsync(appId, assetId, ct);

            A.CallTo(() => assetStore.DeleteByPrefixAsync($"{appId}_{assetId}", ct))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteByPrefixAsync(assetId.ToString(), ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_file_from_store_when_folders_are_used()
        {
            options.FolderPerApp = true;

            await sut.DeleteAsync(appId, assetId, ct);

            A.CallTo(() => assetStore.DeleteByPrefixAsync($"{appId}/", ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_assets_invidually__on_app_deletion()
        {
            var asset1 = new AssetEntity { Id = DomainId.NewGuid() };
            var asset2 = new AssetEntity { Id = DomainId.NewGuid() };

            A.CallTo(() => assetRepository.StreamAll(appId, ct))
                .Returns(new[] { asset1, asset2 }.ToAsyncEnumerable());

            var app = Mocks.App(NamedId.Of(appId, "my-app"));

            await ((IDeleter)sut).DeleteAppAsync(app, ct);

            A.CallTo(() => assetStore.DeleteByPrefixAsync($"{appId}_{asset1.Id}", ct))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteByPrefixAsync($"{appId}_{asset2.Id}", ct))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteByPrefixAsync(asset1.Id.ToString(), ct))
                .MustHaveHappened();

            A.CallTo(() => assetStore.DeleteByPrefixAsync(asset2.Id.ToString(), ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_delete_app_folder_on_app_deletion_when_folders_are_used()
        {
            options.FolderPerApp = true;

            var app = Mocks.App(NamedId.Of(appId, "my-app"));

            await ((IDeleter)sut).DeleteAppAsync(app, ct);

            A.CallTo(() => assetStore.DeleteByPrefixAsync($"{appId}/", ct))
                .MustHaveHappened();
        }

        private string GetFullName(string fileName)
        {
            return fileName
                .Replace("{appId}", appId.ToString(), StringComparison.Ordinal)
                .Replace("{assetId}", assetId.ToString(), StringComparison.Ordinal)
                .Replace("{assetFileVersion}", assetFileVersion.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        }
    }
}
