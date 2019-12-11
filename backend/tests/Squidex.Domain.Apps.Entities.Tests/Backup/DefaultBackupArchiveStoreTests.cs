﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Assets;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class DefaultBackupArchiveStoreTests
    {
        private readonly IAssetStore assetStore = A.Fake<IAssetStore>();
        private readonly Guid backupId = Guid.NewGuid();
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

            await sut.UploadAsync(backupId, stream);

            A.CallTo(() => assetStore.UploadAsync(fileName, stream, true, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_download_archive_using_suffix_for_compatibility()
        {
            var stream = new MemoryStream();

            await sut.DownloadAsync(backupId, stream);

            A.CallTo(() => assetStore.DownloadAsync(fileName, stream, CancellationToken.None))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invoke_asset_store_to_delete_archive_using_suffix_for_compatibility()
        {
            await sut.DeleteAsync(backupId);

            A.CallTo(() => assetStore.DeleteAsync(fileName))
                .MustHaveHappened();
        }
    }
}
