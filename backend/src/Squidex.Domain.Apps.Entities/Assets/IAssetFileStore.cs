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
    public interface IAssetFileStore
    {
        string? GeneratePublicUrl(DomainId appId, DomainId id, long fileVersion, string? suffix);

        Task<long> GetFileSizeAsync(DomainId appId, DomainId id, long fileVersion, string? suffix,
            CancellationToken ct = default);

        Task CopyAsync(string tempFile, DomainId appId, DomainId id, long fileVersion, string? suffix,
            CancellationToken ct = default);

        Task UploadAsync(string tempFile, Stream stream,
            CancellationToken ct = default);

        Task UploadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, bool overwrite = true,
            CancellationToken ct = default);

        Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, string? suffix, Stream stream, BytesRange range = default,
            CancellationToken ct = default);

        Task DeleteAsync(string tempFile,
            CancellationToken ct = default);

        Task DeleteAsync(DomainId appId, DomainId id,
            CancellationToken ct = default);
    }
}
