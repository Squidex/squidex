// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public sealed class MemoryAssetStore : IAssetStore
    {
        private readonly ConcurrentDictionary<string, MemoryStream> streams = new ConcurrentDictionary<string, MemoryStream>();
        private readonly AsyncLock readerLock = new AsyncLock();
        private readonly AsyncLock writerLock = new AsyncLock();

        public string GeneratePublicUrl(string id, long version, string suffix)
        {
            return null;
        }

        public async Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName, nameof(sourceFileName));
            Guard.NotNullOrEmpty(id, nameof(id));

            if (!streams.TryGetValue(sourceFileName, out var sourceStream))
            {
                throw new AssetNotFoundException(sourceFileName);
            }

            using (await readerLock.LockAsync())
            {
                await UploadAsync(id, version, suffix, sourceStream, ct);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var fileName = GetFileName(id, version, suffix);

            if (!streams.TryGetValue(fileName, out var sourceStream))
            {
                throw new AssetNotFoundException(fileName);
            }

            using (await readerLock.LockAsync())
            {
                try
                {
                    await sourceStream.CopyToAsync(stream, 81920, ct);
                }
                finally
                {
                    sourceStream.Position = 0;
                }
            }
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return UploadAsync(GetFileName(id, version, suffix), stream, ct);
        }

        public async Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            var memoryStream = new MemoryStream();

            if (streams.TryAdd(fileName, memoryStream))
            {
                using (await writerLock.LockAsync())
                {
                    try
                    {
                        await stream.CopyToAsync(memoryStream, 81920, ct);
                    }
                    finally
                    {
                        memoryStream.Position = 0;
                    }
                }
            }
            else
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return DeleteAsync(GetFileName(id, version, suffix));
        }

        public Task DeleteAsync(string fileName)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            streams.TryRemove(fileName, out _);

            return TaskHelper.Done;
        }

        private string GetFileName(string id, long version, string suffix)
        {
            return StringExtensions.JoinNonEmpty("_", id, version.ToString(), suffix);
        }
    }
}
