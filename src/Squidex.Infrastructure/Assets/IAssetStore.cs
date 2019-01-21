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
        string GeneratePublicUrl(string id, long version, string suffix);

        Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default);

        Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default);

        Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default);

        Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default);

        Task DeleteAsync(string fileName);

        Task DeleteAsync(string id, long version, string suffix);
    }
}