// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class DefaultAssetFileStore : IAssetFileStore
    {
        private readonly IAssetStore assetStore;

        public DefaultAssetFileStore(IAssetStore assetStore)
        {
            this.assetStore = assetStore;
        }

        public string? GeneratePublicUrl(DomainId appId, DomainId id, long fileVersion)
        {
            var fileName = GetFileName(appId, id, fileVersion);

            return assetStore.GeneratePublicUrl(fileName);
        }

        public async Task<long> GetFileSizeAsync(DomainId appId, DomainId id, long fileVersion, CancellationToken ct = default)
        {
            try
            {
                var fileNameNew = GetFileName(appId, id, fileVersion);

                return await assetStore.GetSizeAsync(fileNameNew, ct);
            }
            catch (AssetNotFoundException)
            {
                var fileNameOld = GetFileName(id, fileVersion);

                return await assetStore.GetSizeAsync(fileNameOld, ct);
            }
        }

        public async Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, Stream stream, BytesRange range = default, CancellationToken ct = default)
        {
            try
            {
                var fileNameNew = GetFileName(appId, id, fileVersion);

                await assetStore.DownloadAsync(fileNameNew, stream, range, ct);
            }
            catch (AssetNotFoundException)
            {
                var fileNameOld = GetFileName(id, fileVersion);

                await assetStore.DownloadAsync(fileNameOld, stream, range, ct);
            }
        }

        public Task UploadAsync(DomainId appId, DomainId id, long fileVersion, Stream stream, CancellationToken ct = default)
        {
            var fileName = GetFileName(appId, id, fileVersion);

            return assetStore.UploadAsync(fileName, stream, true, ct);
        }

        public Task UploadAsync(string tempFile, Stream stream, CancellationToken ct = default)
        {
            return assetStore.UploadAsync(tempFile, stream, false, ct);
        }

        public Task CopyAsync(string tempFile, DomainId appId, DomainId id, long fileVersion, CancellationToken ct = default)
        {
            var fileName = GetFileName(appId, id, fileVersion);

            return assetStore.CopyAsync(tempFile, fileName, ct);
        }

        public async Task DeleteAsync(DomainId appId, DomainId id, long fileVersion)
        {
            var fileNameOld = GetFileName(id, fileVersion);
            var fileNameNew = GetFileName(appId, id, fileVersion);

            await Task.WhenAll(
                assetStore.DeleteAsync(fileNameOld),
                assetStore.DeleteAsync(fileNameNew));
        }

        public Task DeleteAsync(string tempFile)
        {
            return assetStore.DeleteAsync(tempFile);
        }

        private static string GetFileName(DomainId id, long fileVersion)
        {
            return $"{id}_{fileVersion}";
        }

        private static string GetFileName(DomainId appId, DomainId id, long fileVersion)
        {
            return $"{appId}_{id}_{fileVersion}";
        }
    }
}
