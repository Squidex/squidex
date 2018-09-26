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
        string GenerateSourceUrl(string id, long version, string suffix);

        Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default(CancellationToken));

        Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken));

        Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default(CancellationToken));

        Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken));

        Task DeleteAsync(string fileName);

        Task DeleteAsync(string id, long version, string suffix);
    }
}