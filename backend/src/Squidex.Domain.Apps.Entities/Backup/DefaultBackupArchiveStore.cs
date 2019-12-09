// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

        public Task DownloadAsync(Guid backupId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(backupId);

            return assetStore.DownloadAsync(fileName, stream, ct);
        }

        public Task UploadAsync(Guid backupId, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(backupId);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task DeleteAsync(Guid backupId)
        {
            var fileName = GetFileName(backupId);

            return assetStore.DeleteAsync(fileName);
        }

        private string GetFileName(Guid backupId)
        {
            return $"{backupId}_0";
        }
    }
}
