// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetStore
    {
        string? GeneratePublicUrl(string fileName);

        Task<long> GetSizeAsync(string fileName, CancellationToken ct = default);

        Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default);

        Task DownloadAsync(string fileName, Stream stream, BytesRange range = default, CancellationToken ct = default);

        Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default);

        Task DeleteAsync(string fileName);
    }
}