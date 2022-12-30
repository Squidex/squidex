// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Apps;

public class DefaultAppImageStoreTests : GivenContext
{
    private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
    private readonly string fileNameDefault;
    private readonly string fileNameFolder;
    private readonly AssetOptions options = new AssetOptions();
    private readonly DefaultAppImageStore sut;

    public DefaultAppImageStoreTests()
    {
        fileNameDefault = AppId.Id.ToString();
        fileNameFolder = $"{AppId.Id}/thumbnail";

        sut = new DefaultAppImageStore(assetStore, Options.Create(options));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_invoke_asset_store_to_upload_archive(bool folderPerApp)
    {
        var stream = new MemoryStream();

        options.FolderPerApp = folderPerApp;

        var fileName = GetFileName(folderPerApp);

        await sut.UploadAsync(AppId.Id, stream, CancellationToken);

        A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken))
            .MustHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_invoke_asset_store_to_download_archive(bool folderPerApp)
    {
        var stream = new MemoryStream();

        options.FolderPerApp = folderPerApp;

        var fileName = GetFileName(folderPerApp);

        await sut.DownloadAsync(AppId.Id, stream, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync(fileName, stream, default, CancellationToken))
            .MustHaveHappened();
    }

    private string GetFileName(bool folderPerApp)
    {
        return folderPerApp ? fileNameFolder : fileNameDefault;
    }
}
