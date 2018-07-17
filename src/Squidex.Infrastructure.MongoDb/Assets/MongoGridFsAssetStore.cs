// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Squidex.Infrastructure.Assets
{
    public sealed class MongoGridFsAssetStore : IAssetStore, IInitializable
    {
        private const int BufferSize = 81920;
        private readonly IGridFSBucket<string> bucket;

        public MongoGridFsAssetStore(IGridFSBucket<string> bucket)
        {
            Guard.NotNull(bucket, nameof(bucket));

            this.bucket = bucket;
        }

        public void Initialize()
        {
            try
            {
                bucket.Database.ListCollections();
            }
            catch (MongoException ex)
            {
                throw new ConfigurationException($"Cannot connect to Mongo GridFS bucket '${bucket.Options.BucketName}'.", ex);
            }
        }

        public string GenerateSourceUrl(string id, long version, string suffix)
        {
            return "UNSUPPORTED";
        }

        public async Task CopyAsync(string name, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var target = GetFileName(id, version, suffix);

                using (var readStream = await bucket.OpenDownloadStreamAsync(name, cancellationToken: ct))
                {
                    await bucket.UploadFromStreamAsync(target, target, readStream, cancellationToken: ct);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var name = GetFileName(id, version, suffix);

                using (var readStream = await bucket.OpenDownloadStreamAsync(name, cancellationToken: ct))
                {
                    await readStream.CopyToAsync(stream, BufferSize);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
            }
        }

        public Task UploadAsync(string name, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadFileCoreAsync(name, stream, ct);
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadFileCoreAsync(GetFileName(id, version, suffix), stream, ct);
        }

        public Task DeleteAsync(string name)
        {
            return DeleteCoreAsync(name);
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            return DeleteCoreAsync(GetFileName(id, version, suffix));
        }

        private async Task DeleteCoreAsync(string id)
        {
            try
            {
                await bucket.DeleteAsync(id);
            }
            catch (GridFSFileNotFoundException)
            {
                return;
            }
        }

        private Task UploadFileCoreAsync(string id, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return bucket.UploadFromStreamAsync(id, id, stream, cancellationToken: ct);
        }

        private static string GetFileName(string id, long version, string suffix)
        {
            return string.Join("_", new[] { id, version.ToString(), suffix }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}