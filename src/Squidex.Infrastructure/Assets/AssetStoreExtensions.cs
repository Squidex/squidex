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

namespace Squidex.Infrastructure.Assets
{
    public static class AssetStoreExtensions
    {
        public static string GeneratePublicUrl(this IAssetStore store, Guid id, long version, string suffix)
        {
            return store.GeneratePublicUrl(id.ToString(), version, suffix);
        }

        public static string GeneratePublicUrl(this IAssetStore store, string id, long version, string suffix)
        {
            return store.GeneratePublicUrl(GetFileName(id, version, suffix));
        }

        public static Task CopyAsync(this IAssetStore store, string sourceFileName, Guid id, long version, string suffix, CancellationToken ct = default)
        {
            return store.CopyAsync(sourceFileName, id.ToString(), version, suffix, ct);
        }

        public static Task CopyAsync(this IAssetStore store, string sourceFileName, string id, long version, string suffix, CancellationToken ct = default)
        {
            return store.CopyAsync(sourceFileName, GetFileName(id, version, suffix), ct);
        }

        public static Task DownloadAsync(this IAssetStore store, Guid id, long version, string suffix, Stream stream, CancellationToken ct = default)
        {
            return store.DownloadAsync(id.ToString(), version, suffix, stream, ct);
        }

        public static Task DownloadAsync(this IAssetStore store, string id, long version, string suffix, Stream stream, CancellationToken ct = default)
        {
            return store.DownloadAsync(GetFileName(id, version, suffix), stream, ct);
        }

        public static Task UploadAsync(this IAssetStore store, Guid id, long version, string suffix, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            return store.UploadAsync(id.ToString(), version, suffix, stream, overwrite, ct);
        }

        public static Task UploadAsync(this IAssetStore store, string id, long version, string suffix, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            return store.UploadAsync(GetFileName(id, version, suffix), stream, overwrite, ct);
        }

        public static Task DeleteAsync(this IAssetStore store, Guid id, long version, string suffix)
        {
            return store.DeleteAsync(id.ToString(), version, suffix);
        }

        public static Task DeleteAsync(this IAssetStore store, string id, long version, string suffix)
        {
            return store.DeleteAsync(GetFileName(id, version, suffix));
        }

        public static string GetFileName(string id, long version, string suffix = null)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return StringExtensions.JoinNonEmpty("_", id, version.ToString(), suffix);
        }
    }
}
