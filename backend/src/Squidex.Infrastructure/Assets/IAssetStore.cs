// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetStore
    {
        string? GeneratePublicUrl(string fileName);

        string? GeneratePublicUrl(Guid id, long version, string? suffix)
        {
            return GeneratePublicUrl(id.ToString(), version, suffix);
        }

        string? GeneratePublicUrl(string id, long version, string? suffix)
        {
            return GeneratePublicUrl(AssetStoreHelper.GetFileName(id, version, suffix));
        }

        Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default);

        Task CopyAsync(string sourceFileName, Guid id, long version, string? suffix, CancellationToken ct = default)
        {
            return CopyAsync(sourceFileName, id.ToString(), version, suffix, ct);
        }

        Task CopyAsync(string sourceFileName, string id, long version, string? suffix, CancellationToken ct = default)
        {
            return CopyAsync(sourceFileName, AssetStoreHelper.GetFileName(id, version, suffix), ct);
        }

        Task DownloadAsync(string fileName, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(Guid id, long version, string? suffix, Stream stream, CancellationToken ct = default)
        {
            return DownloadAsync(id.ToString(), version, suffix, stream, ct);
        }

        Task DownloadAsync(string id, long version, string? suffix, Stream stream, CancellationToken ct = default)
        {
            return DownloadAsync(AssetStoreHelper.GetFileName(id, version, suffix), stream, ct);
        }

        Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default);

        Task UploadAsync(Guid id, long version, string? suffix, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            return UploadAsync(id.ToString(), version, suffix, stream, overwrite, ct);
        }

        Task UploadAsync(string id, long version, string? suffix, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            return UploadAsync(AssetStoreHelper.GetFileName(id, version, suffix), stream, overwrite, ct);
        }

        Task DeleteAsync(string fileName);

        Task DeleteAsync(Guid id, long version, string? suffix)
        {
            return DeleteAsync(id.ToString(), version, suffix);
        }

        Task DeleteAsync(string id, long version, string? suffix)
        {
            return DeleteAsync(AssetStoreHelper.GetFileName(id, version, suffix));
        }
    }
}