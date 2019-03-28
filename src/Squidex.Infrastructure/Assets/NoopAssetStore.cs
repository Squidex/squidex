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
    public sealed class NoopAssetStore : IAssetStore
    {
        public string GeneratePublicUrl(string id, long version, string suffix)
        {
            return null;
        }

        public Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(string fileName)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            throw new NotSupportedException();
        }
    }
}
