// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class DefaultAssetFileStore : IAssetFileStore
    {
        private readonly IAssetStore assetStore;
        private readonly AssetOptions options;

        public DefaultAssetFileStore(IAssetStore assetStore,
            IOptions<AssetOptions> options)
        {
            this.assetStore = assetStore;

            this.options = options.Value;
        }

        public string? GeneratePublicUrl(DomainId appId, DomainId id, long fileVersion, string? suffix)
        {
            var fileName = GetFileName(appId, id, fileVersion, suffix);

            return assetStore.GeneratePublicUrl(fileName);
        }

        public async Task<long> GetFileSizeAsync(DomainId appId, DomainId id, long fileVersion, string? suffix,
            CancellationToken ct = default)
        {
            try
            {
                var fileNameNew = GetFileName(appId, id, fileVersion, suffix);

                return await assetStore.GetSizeAsync(fileNameNew, ct);
            }
            catch (AssetNotFoundException) when (!options.FolderPerApp)
            {
                var fileNameOld = GetFileName(id, fileVersion, suffix);

                return await assetStore.GetSizeAsync(fileNameOld, ct);
            }
        }

        public async Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, BytesRange range = default,
            CancellationToken ct = default)
        {
            try
            {
                var fileNameNew = GetFileName(appId, id, fileVersion, suffix);

                await assetStore.DownloadAsync(fileNameNew, stream, range, ct);
            }
            catch (AssetNotFoundException) when (!options.FolderPerApp)
            {
                var fileNameOld = GetFileName(id, fileVersion, suffix);

                await assetStore.DownloadAsync(fileNameOld, stream, range, ct);
            }
        }

        public Task UploadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, bool overwrite = true,
            CancellationToken ct = default)
        {
            var fileName = GetFileName(appId, id, fileVersion, suffix);

            return assetStore.UploadAsync(fileName, stream, overwrite, ct);
        }

        public Task UploadAsync(string tempFile, Stream stream,
            CancellationToken ct = default)
        {
            return assetStore.UploadAsync(tempFile, stream, false, ct);
        }

        public Task CopyAsync(string tempFile, DomainId appId, DomainId id, long fileVersion, string? suffix,
            CancellationToken ct = default)
        {
            var fileName = GetFileName(appId, id, fileVersion, suffix);

            return assetStore.CopyAsync(tempFile, fileName, ct);
        }

        public Task DeleteAsync(DomainId appId, DomainId id, long fileVersion, string? suffix)
        {
            var fileNameOld = GetFileName(id, fileVersion, suffix);
            var fileNameNew = GetFileName(appId, id, fileVersion, suffix);

            if (options.FolderPerApp)
            {
                return assetStore.DeleteAsync(fileNameNew);
            }
            else
            {
                return Task.WhenAll(
                    assetStore.DeleteAsync(fileNameOld),
                    assetStore.DeleteAsync(fileNameNew));
            }
        }

        public Task DeleteAsync(string tempFile)
        {
            return assetStore.DeleteAsync(tempFile);
        }

        private static string GetFileName(DomainId id, long fileVersion, string? suffix)
        {
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                return $"{id}_{fileVersion}_{suffix}";
            }
            else
            {
                return $"{id}_{fileVersion}";
            }
        }

        private string GetFileName(DomainId appId, DomainId id, long fileVersion, string? suffix)
        {
            if (options.FolderPerApp)
            {
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    return $"derived/{appId}/{id}_{fileVersion}_{suffix}";
                }
                else
                {
                    return $"{appId}/{id}_{fileVersion}";
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(suffix))
                {
                    return $"{appId}_{id}_{fileVersion}_{suffix}";
                }
                else
                {
                    return $"{appId}_{id}_{fileVersion}";
                }
            }
        }
    }
}
