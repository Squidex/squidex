﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public class DefaultAssetFileStoreTests : GivenContext
{
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly long assetFileVersion = 21;
    private readonly AssetOptions options = new AssetOptions();
    private readonly DefaultAssetFileStore sut;

    public static readonly TheoryData<bool, string, string> PathCases = new TheoryData<bool, string, string>
    {
        { true, "resize=100", "{appId}/{assetId}_{assetFileVersion}_resize=100" },
        { true, string.Empty, "{appId}/{assetId}_{assetFileVersion}" },
        { false, "resize=100", "{appId}_{assetId}_{assetFileVersion}_resize=100" },
        { false, string.Empty, "{appId}_{assetId}_{assetFileVersion}" },
    };

    public static readonly TheoryData<string, string> PathCasesOld = new TheoryData<string, string>
    {
        { "resize=100", "{assetId}_{assetFileVersion}_resize=100" },
        { string.Empty, "{assetId}_{assetFileVersion}" },
    };

    public DefaultAssetFileStoreTests()
    {
        sut = new DefaultAssetFileStore(assetStore, assetRepository, Options.Create(options));
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

        var actual = sut.GeneratePublicUrl(AppId.Id, assetId, assetFileVersion, suffix);

        Assert.Equal(url, actual);
    }

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task Should_get_file_size_from_store(bool folderPerApp, string? suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        options.FolderPerApp = folderPerApp;

        var size = 1024L;

        A.CallTo(() => assetStore.GetSizeAsync(fullName, CancellationToken))
            .Returns(size);

        var actual = await sut.GetFileSizeAsync(AppId.Id, assetId, assetFileVersion, suffix, CancellationToken);

        Assert.Equal(size, actual);
    }

    [Theory]
    [MemberData(nameof(PathCasesOld))]
    public async Task Should_get_file_size_from_store_with_old_file_name_if_new_name_not_found(string? suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        var size = 1024L;

        A.CallTo(() => assetStore.GetSizeAsync(A<string>._, CancellationToken))
            .Throws(new AssetNotFoundException(assetId.ToString()));

        A.CallTo(() => assetStore.GetSizeAsync(fullName, CancellationToken))
            .Returns(size);

        var actual = await sut.GetFileSizeAsync(AppId.Id, assetId, assetFileVersion, suffix, CancellationToken);

        Assert.Equal(size, actual);
    }

    [Fact]
    public async Task Should_upload_temporary_file_to_store()
    {
        var stream = new MemoryStream();

        await sut.UploadAsync("Temp", stream, CancellationToken);

        A.CallTo(() => assetStore.UploadAsync("Temp", stream, false, CancellationToken))
            .MustHaveHappened();
    }

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task Should_upload_file_to_store(bool folderPerApp, string? suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        options.FolderPerApp = folderPerApp;

        var stream = new MemoryStream();

        await sut.UploadAsync(AppId.Id, assetId, assetFileVersion, suffix, stream, true, CancellationToken);

        A.CallTo(() => assetStore.UploadAsync(fullName, stream, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_download_temporary_file_to_store()
    {
        var stream = new MemoryStream();

        await sut.DownloadAsync("Temp", stream, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync("Temp", stream, default, CancellationToken))
            .MustHaveHappened();
    }

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task Should_download_file_from_store(bool folderPerApp, string? suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        options.FolderPerApp = folderPerApp;

        var stream = new MemoryStream();

        await sut.DownloadAsync(AppId.Id, assetId, assetFileVersion, suffix, stream, default, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_download_file_from_store_with_folder_only_if_configured()
    {
        options.FolderPerApp = true;

        var stream = new MemoryStream();

        A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, CancellationToken))
            .Throws(new AssetNotFoundException(assetId.ToString())).Once();

        await Assert.ThrowsAsync<AssetNotFoundException>(() => sut.DownloadAsync(AppId.Id, assetId, assetFileVersion, null, stream, default, CancellationToken));

        A.CallTo(() => assetStore.DownloadAsync(A<string>._, stream, default, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [MemberData(nameof(PathCasesOld))]
    public async Task Should_download_file_from_store_with_old_file_name_if_new_name_not_found(string suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        var stream = new MemoryStream();

        A.CallTo(() => assetStore.DownloadAsync(A<string>.That.Matches(x => x != fileName), stream, default, CancellationToken))
            .Throws(new AssetNotFoundException(assetId.ToString())).Once();

        await sut.DownloadAsync(AppId.Id, assetId, assetFileVersion, suffix, stream, default, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync(fullName, stream, default, CancellationToken))
            .MustHaveHappened();
    }

    [Theory]
    [MemberData(nameof(PathCases))]
    public async Task Should_copy_file_to_store(bool folderPerApp, string? suffix, string fileName)
    {
        var fullName = GetFullName(fileName);

        options.FolderPerApp = folderPerApp;

        await sut.CopyAsync("Temp", AppId.Id, assetId, assetFileVersion, suffix, CancellationToken);

        A.CallTo(() => assetStore.CopyAsync("Temp", fullName, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_temporary_file_from_store()
    {
        await sut.DeleteAsync("Temp", CancellationToken);

        A.CallTo(() => assetStore.DeleteAsync("Temp", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_file_from_store()
    {
        await sut.DeleteAsync(AppId.Id, assetId, CancellationToken);

        A.CallTo(() => assetStore.DeleteByPrefixAsync($"{AppId.Id}_{assetId}", CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetStore.DeleteByPrefixAsync(assetId.ToString(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_file_from_store_when_folders_are_used()
    {
        options.FolderPerApp = true;

        await sut.DeleteAsync(AppId.Id, assetId, CancellationToken);

        A.CallTo(() => assetStore.DeleteByPrefixAsync($"{AppId.Id}/{assetId}", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_assets_invidually__on_app_deletion()
    {
        var asset1 = CreateAsset();
        var asset2 = CreateAsset();

        A.CallTo(() => assetRepository.StreamAll(AppId.Id, CancellationToken))
            .Returns(new[] { asset1, asset2 }.ToAsyncEnumerable());

        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => assetStore.DeleteByPrefixAsync($"{AppId.Id}_{asset1.Id}", CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetStore.DeleteByPrefixAsync($"{AppId.Id}_{asset2.Id}", CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetStore.DeleteByPrefixAsync(asset1.Id.ToString(), CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => assetStore.DeleteByPrefixAsync(asset2.Id.ToString(), CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_app_folder_on_app_deletion_when_folders_are_used()
    {
        options.FolderPerApp = true;

        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => assetStore.DeleteByPrefixAsync($"{AppId.Id}/", CancellationToken))
            .MustHaveHappened();
    }

    private string GetFullName(string fileName)
    {
        return fileName
            .Replace("{appId}", AppId.Id.ToString(), StringComparison.Ordinal)
            .Replace("{assetId}", assetId.ToString(), StringComparison.Ordinal)
            .Replace("{assetFileVersion}", assetFileVersion.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}
