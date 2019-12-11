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
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class DefaultAssetFileStore : IAssetFileStore
    {
        private readonly IAssetStore assetStore;

        public DefaultAssetFileStore(IAssetStore assetStore)
        {
            this.assetStore = assetStore;
        }

        public string? GeneratePublicUrl(Guid id, long fileVersion)
        {
            var fileName = GetFileName(id, fileVersion);

            return assetStore.GeneratePublicUrl(fileName);
        }

        public Task UploadAsync(Guid id, long fileVersion, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(id, fileVersion);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task UploadAsync(string tempFile, Stream stream, CancellationToken ct = default)
        {
            return assetStore.UploadAsync(tempFile, stream, false, ct);
        }

        public Task CopyAsync(string tempFile, Guid id, long fileVersion, CancellationToken ct = default)
        {
            var fileName = GetFileName(id, fileVersion);

            return assetStore.CopyAsync(tempFile, fileName, ct);
        }

        public Task DownloadAsync(Guid id, long fileVersion, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(id, fileVersion);

            return assetStore.DownloadAsync(fileName, stream, ct);
        }

        public Task DeleteAsync(Guid id, long fileVersion)
        {
            var fileName = GetFileName(id, fileVersion);

            return assetStore.DeleteAsync(fileName);
        }

        public Task DeleteAsync(string tempFile)
        {
            return assetStore.DeleteAsync(tempFile);
        }

        private static string GetFileName(Guid id, long fileVersion)
        {
            return $"{id}_{fileVersion}";
        }
    }
}
