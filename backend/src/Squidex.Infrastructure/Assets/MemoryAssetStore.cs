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
    public class MemoryAssetStore : IAssetStore
    {
        private readonly ConcurrentDictionary<string, MemoryStream> streams = new ConcurrentDictionary<string, MemoryStream>();
        private readonly AsyncLock readerLock = new AsyncLock();
        private readonly AsyncLock writerLock = new AsyncLock();

        public string? GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public async Task<long> GetSizeAsync(string fileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            if (!streams.TryGetValue(fileName, out var sourceStream))
            {
                throw new AssetNotFoundException(fileName);
            }

            using (await readerLock.LockAsync())
            {
                return sourceStream.Length;
            }
        }

        public virtual async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName, nameof(sourceFileName));
            Guard.NotNullOrEmpty(targetFileName, nameof(targetFileName));

            if (!streams.TryGetValue(sourceFileName, out var sourceStream))
            {
                throw new AssetNotFoundException(sourceFileName);
            }

            using (await readerLock.LockAsync())
            {
                await UploadAsync(targetFileName, sourceStream, false, ct);
            }
        }

        public virtual async Task DownloadAsync(string fileName, Stream stream, BytesRange range = default, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNull(stream, nameof(stream));

            if (!streams.TryGetValue(fileName, out var sourceStream))
            {
                throw new AssetNotFoundException(fileName);
            }

            using (await readerLock.LockAsync())
            {
                try
                {
                    await sourceStream.CopyToAsync(stream, range, ct);
                }
                finally
                {
                    sourceStream.Position = 0;
                }
            }
        }

        public virtual async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNull(stream, nameof(stream));

            var memoryStream = new MemoryStream();

            async Task CopyAsync()
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

            if (overwrite)
            {
                await CopyAsync();

                streams[fileName] = memoryStream;
            }
            else if (streams.TryAdd(fileName, memoryStream))
            {
                await CopyAsync();
            }
            else
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public virtual Task DeleteAsync(string fileName)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            streams.TryRemove(fileName, out _);

            return Task.CompletedTask;
        }
    }
}
