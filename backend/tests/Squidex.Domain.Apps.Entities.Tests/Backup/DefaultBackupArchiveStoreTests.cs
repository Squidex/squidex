// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup;

public class DefaultBackupArchiveStoreTests : GivenContext
{
    private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
    private readonly DomainId backupId = DomainId.NewGuid();
    private readonly string fileName;
    private readonly DefaultBackupArchiveStore sut;

    public DefaultBackupArchiveStoreTests()
    {
        fileName = $"{backupId}_0";

        sut = new DefaultBackupArchiveStore(assetStore);
    }

    [Fact]
    public async Task Should_invoke_asset_store_to_upload_archive_using_suffix_for_compatibility()
    {
        var stream = new MemoryStream();

        await sut.UploadAsync(backupId, stream, CancellationToken);

        A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_asset_store_to_download_archive_using_suffix_for_compatibility()
    {
        var stream = new MemoryStream();

        await sut.DownloadAsync(backupId, stream, CancellationToken);

        A.CallTo(() => assetStore.DownloadAsync(fileName, stream, default, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_asset_store_to_delete_archive_using_suffix_for_compatibility()
    {
        await sut.DeleteAsync(backupId, CancellationToken);

        A.CallTo(() => assetStore.DeleteAsync(fileName, CancellationToken))
            .MustHaveHappened();
    }
}
