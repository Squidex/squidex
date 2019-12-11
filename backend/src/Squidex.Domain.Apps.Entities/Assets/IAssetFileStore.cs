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

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetFileStore
    {
        string? GeneratePublicUrl(Guid id, long fileVersion);

        Task CopyAsync(string tempFile, Guid id, long fileVersion, CancellationToken ct = default);

        Task UploadAsync(string tempFile, Stream stream, CancellationToken ct = default);

        Task UploadAsync(Guid id, long fileVersion, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(Guid id, long fileVersion, Stream stream, CancellationToken ct = default);

        Task DeleteAsync(string tempFile);

        Task DeleteAsync(Guid id, long fileVersion);
    }
}
