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

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetFileStore
    {
        string? GeneratePublicUrl(DomainId appId, DomainId id, long fileVersion);

        Task<long> GetFileSizeAsync(DomainId appId, DomainId id, long fileVersion, CancellationToken ct = default);

        Task CopyAsync(string tempFile, DomainId appId, DomainId id, long fileVersion, CancellationToken ct = default);

        Task UploadAsync(string tempFile, Stream stream, CancellationToken ct = default);

        Task UploadAsync(DomainId appId, DomainId id, long fileVersion, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(DomainId appId, DomainId id, long fileVersion, Stream stream, BytesRange range = default, CancellationToken ct = default);

        Task DeleteAsync(string tempFile);

        Task DeleteAsync(DomainId appId, DomainId id, long fileVersion);
    }
}
