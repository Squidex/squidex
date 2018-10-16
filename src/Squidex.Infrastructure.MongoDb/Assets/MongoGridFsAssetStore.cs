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
using MongoDB.Bson;
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

        public async Task InitializeAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                await bucket.Database.ListCollectionsAsync(cancellationToken: ct);
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

        public async Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var target = GetFileName(id, version, suffix);

                using (var readStream = await bucket.OpenDownloadStreamAsync(sourceFileName, cancellationToken: ct))
                {
                    await UploadFileCoreAsync(target, readStream, ct);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var name = GetFileName(id, version, suffix);

                using (var readStream = await bucket.OpenDownloadStreamAsync(name, cancellationToken: ct))
                {
                    await readStream.CopyToAsync(stream, BufferSize, ct);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Id={id}, Version={version}", ex);
            }
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadFileCoreAsync(GetFileName(id, version, suffix), stream, ct);
        }

        public Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadFileCoreAsync(fileName, stream, ct);
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            return DeleteCoreAsync(GetFileName(id, version, suffix));
        }

        public Task DeleteAsync(string fileName)
        {
            return DeleteCoreAsync(fileName);
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

        private async Task UploadFileCoreAsync(string id, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                await bucket.UploadFromStreamAsync(id, id, stream, cancellationToken: ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new AssetAlreadyExistsException(id);
            }
            catch (MongoBulkWriteException<BsonDocument> ex) when (ex.WriteErrors.Any(x => x.Category == ServerErrorCategory.DuplicateKey))
            {
                throw new AssetAlreadyExistsException(id);
            }
        }

        private static string GetFileName(string id, long version, string suffix)
        {
            return string.Join("_", new[] { id, version.ToString(), suffix }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}