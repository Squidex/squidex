﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class DefaultBackupArchiveStore : IBackupArchiveStore
    {
        private readonly IAssetStore assetStore;

        public DefaultBackupArchiveStore(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore, nameof(assetStore));

            this.assetStore = assetStore;
        }

        public Task DownloadAsync(DomainId backupId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(backupId);

            return assetStore.DownloadAsync(fileName, stream, default, ct);
        }

        public Task UploadAsync(DomainId backupId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(backupId);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task DeleteAsync(DomainId backupId)
        {
            var fileName = GetFileName(backupId);

            return assetStore.DeleteAsync(fileName);
        }

        private static string GetFileName(DomainId backupId)
        {
            return $"{backupId}_0";
        }
    }
}
