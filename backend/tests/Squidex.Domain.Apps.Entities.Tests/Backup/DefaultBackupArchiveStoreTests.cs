// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Assets;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class DefaultBackupArchiveStoreTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly DomainId backupId = DomainId.NewGuid();
        private readonly string fileName;
        private readonly DefaultBackupArchiveStore sut;

        public DefaultBackupArchiveStoreTests()
        {
            ct = cts.Token;

            fileName = $"{backupId}_0";

            sut = new DefaultBackupArchiveStore(assetStore);
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_upload_archive_using_suffix_for_compatibility()
        {
            var stream = new MemoryStream();

            await sut.UploadAsync(backupId, stream, ct);

            A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_archive_using_suffix_for_compatibility()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(backupId, stream, ct);

            A.CallTo(() => assetStore.DownloadAsync(fileName, stream, default, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_delete_archive_using_suffix_for_compatibility()
        {
            await sut.DeleteAsync(backupId, ct);

            A.CallTo(() => assetStore.DeleteAsync(fileName, ct))
                .MustHaveHappened();
        }
    }
}
