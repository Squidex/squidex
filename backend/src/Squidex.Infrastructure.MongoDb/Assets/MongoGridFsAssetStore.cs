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
        private static readonly GridFSDownloadOptions DownloadDefault = new GridFSDownloadOptions();
        private static readonly GridFSDownloadOptions DownloadSeekable = new GridFSDownloadOptions { Seekable = true };
        private readonly IGridFSBucket<string> bucket;

        public MongoGridFsAssetStore(IGridFSBucket<string> bucket)
        {
            Guard.NotNull(bucket, nameof(bucket));

            this.bucket = bucket;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                await bucket.Database.ListCollectionsAsync(cancellationToken: ct);
            }
            catch (MongoException ex)
            {
                throw new ConfigurationException($"Cannot connect to Mongo GridFS bucket '{bucket.Options.BucketName}'.", ex);
            }
        }

        public string? GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public async Task<long> GetSizeAsync(string fileName, CancellationToken ct = default)
        {
            var name = GetFileName(fileName, nameof(fileName));

            var query = await bucket.FindAsync(Builders<GridFSFileInfo<string>>.Filter.Eq(x => x.Id, name), cancellationToken: ct);

            var file = await query.FirstOrDefaultAsync(cancellationToken: ct);

            if (file == null)
            {
                throw new AssetNotFoundException(fileName);
            }

            return file.Length;
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(targetFileName, nameof(targetFileName));

            try
            {
                var sourceName = GetFileName(sourceFileName, nameof(sourceFileName));

                await using (var readStream = await bucket.OpenDownloadStreamAsync(sourceName, cancellationToken: ct))
                {
                    await UploadAsync(targetFileName, readStream, false, ct);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, BytesRange range, CancellationToken ct = default)
        {
            Guard.NotNull(stream, nameof(stream));

            try
            {
                var name = GetFileName(fileName, nameof(fileName));

                var options = range.IsDefined ? DownloadSeekable : DownloadDefault;

                using (var readStream = await bucket.OpenDownloadStreamAsync(name, options, ct))
                {
                    await readStream.CopyToAsync(stream, range, ct);
                }
            }
            catch (GridFSFileNotFoundException ex)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNull(stream, nameof(stream));

            try
            {
                var name = GetFileName(fileName, nameof(fileName));

                if (overwrite)
                {
                    await DeleteAsync(fileName);
                }

                await bucket.UploadFromStreamAsync(name, name, stream, cancellationToken: ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new AssetAlreadyExistsException(fileName);
            }
            catch (MongoBulkWriteException<BsonDocument> ex) when (ex.WriteErrors.Any(x => x.Category == ServerErrorCategory.DuplicateKey))
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public async Task DeleteAsync(string fileName)
        {
            try
            {
                var name = GetFileName(fileName, nameof(fileName));

                await bucket.DeleteAsync(name);
            }
            catch (GridFSFileNotFoundException)
            {
                return;
            }
        }

        private static string GetFileName(string fileName, string parameterName)
        {
            Guard.NotNullOrEmpty(fileName, parameterName);

            return fileName;
        }
    }
}